using System.Collections.Generic;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal class InlineTableParser : IDocumentationParser
{
    public IList<IDocumentationNode> Parse(INamedTypeSymbol namedTypeSymbol, DocumentationOptionsDataModel options,
        IList<string> filePaths)
    {
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default);

        IDocumentationNode root = namedTypeSymbol.CreateDocumentationNode(options);

        RecurseNodes(namedTypeSymbol, string.Empty, visited, root);

        return [root];
    }

    private void RecurseNodes(INamedTypeSymbol? current, string prefix, HashSet<INamedTypeSymbol> visited, IDocumentationNode root)
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
            ParseProperty(prefix, visited, root, member);
        }
    }

    private void ParseProperty(string prefix, HashSet<INamedTypeSymbol> visited, IDocumentationNode node,
        IPropertySymbol property)
    {
        // Only public properties
        if (property.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        // Skip if marked with ExcludeFromDocumentationAttribute (compare by full name string to avoid a type reference)
        AttributeData? excludeAttribute = property.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "DotAutoDocConfig.Core.ComponentModel.Attributes.ExcludeFromDocumentationAttribute");
        if (excludeAttribute != null)
        {
            return;
        }

        string formattedName = property.Name;
        string separator = ":"; // JsonShort notation uses colon
        string parameterName = string.IsNullOrEmpty(prefix) ? formattedName : prefix + separator + formattedName;

        ITypeSymbol propertyType = property.Type;

        // If array or generic enumerable, get the element type
        if (propertyType is IArrayTypeSymbol arrayType)
        {
            propertyType = arrayType.ElementType;
        }
        else if (propertyType is INamedTypeSymbol named && named.IsGenericType)
        {
            // try to find IEnumerable<T>
            ITypeSymbol? firstArg = named.TypeArguments.FirstOrDefault();
            if (firstArg != null && named.AllInterfaces.Any(i => i.MetadataName.Contains("IEnumerable")))
            {
                propertyType = firstArg;
            }
        }

        // If the property type is a class/record (and not system/primitive), recurse into it
        if (propertyType is INamedTypeSymbol namedType && namedType.IsCustomClass())
        {
            // Recurse into child properties using the parameter name as prefix
            RecurseNodes(namedType, parameterName, visited, node);
            return;
        }

        // Otherwise emit a documentation entry for this property
        ITableRowNode tableRow = property.CreateTableRowNode(parameterName);
        node.Table.Body.TableRows.Add(tableRow);
    }
}
