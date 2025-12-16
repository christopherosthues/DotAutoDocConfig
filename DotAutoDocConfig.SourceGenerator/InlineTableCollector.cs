using System.Collections.Generic;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator;

internal class InlineTableCollector : TableCollectorBase
{
    public static List<DocumentationDataModel> CollectDocumentationEntries(INamedTypeSymbol root)
    {
        List<DocumentationDataModel> result = [];
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default);

        Recurse(root, string.Empty, visited, root, result);
        return result;
    }

    private static void Recurse(INamedTypeSymbol? current, string prefix, HashSet<INamedTypeSymbol> visited, INamedTypeSymbol root, List<DocumentationDataModel> result)
    {
        if (current is null)
        {
            return;
        }

        if (!visited.Add(current))
        {
            return; // prevent cycles
        }

        foreach (IPropertySymbol member in current.GetMembers().OfType<IPropertySymbol>())
        {
            // Only public properties
            if (member.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            // Skip if marked with ExcludeFromDocumentationAttribute (compare by full name string to avoid a type reference)
            AttributeData? excludeAttribute = member.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "DotAutoDocConfig.Core.ComponentModel.Attributes.ExcludeFromDocumentationAttribute");
            if (excludeAttribute != null)
            {
                continue;
            }

            string formattedName = member.Name;
            string separator = ":"; // JsonShort notation uses colon
            string parameterName = string.IsNullOrEmpty(prefix) ? formattedName : prefix + separator + formattedName;

            ITypeSymbol propType = member.Type;

            // If array or generic enumerable, get the element type
            if (propType is IArrayTypeSymbol arrayType)
            {
                propType = arrayType.ElementType;
            }
            else if (propType is INamedTypeSymbol named && named.IsGenericType)
            {
                // try to find IEnumerable<T>
                ITypeSymbol? firstArg = named.TypeArguments.FirstOrDefault();
                if (firstArg != null && named.AllInterfaces.Any(i => i.MetadataName.Contains("IEnumerable")))
                {
                    propType = firstArg;
                }
            }

            // If the property type is a class/record (and not system/primitive), recurse into it
            if (propType is INamedTypeSymbol namedType && ShouldRecurseInto(namedType))
            {
                // Recurse into child properties using the parameter name as prefix
                Recurse(namedType, parameterName, visited, root, result);
                continue; // do not emit the parent as a simple property
            }

            // Otherwise emit a documentation entry for this property
            DocumentationDataModel model =
                DocumentationDataModelFactory.CreateLeafDocumentationDataModel(root, parameterName, member);

            result.Add(model);
        }
    }
}
