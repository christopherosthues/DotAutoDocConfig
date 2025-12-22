using System.Collections.Generic;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal class SeparateTableParser : IDocumentationParser
{
    public IList<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> Parse(INamedTypeSymbol namedTypeSymbol, bool includeNamespaces)
    {
        List<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> allNodes = [];
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default);

        const bool isRoot = true;
        IDocumentationNode root = namedTypeSymbol.CreateDocumentationNode(includeNamespaces, isRoot);

        allNodes.Add((namedTypeSymbol, root));

        RecurseNodes(namedTypeSymbol, visited, root, allNodes, includeNamespaces);

        return allNodes;
    }

    private static void RecurseNodes(INamedTypeSymbol? current, HashSet<INamedTypeSymbol> visited,
        IDocumentationNode node, IList<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> allNodes,
        bool includeNamespaces)
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
            ParseProperty(visited, node, member, allNodes, includeNamespaces);
        }
    }

    private static void ParseProperty(HashSet<INamedTypeSymbol> visited,
        IDocumentationNode node, IPropertySymbol property,
        IList<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> allNodes, bool includeNamespaces)
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

        string parameterName = property.Name;

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
            ITableRowNode customTableRow = property.CreateTableRowNodeWithLink(parameterName);
            node.Table.Body.TableRows.Add(customTableRow);

            const bool isRoot = false;
            IDocumentationNode root = namedType.CreateDocumentationNode(includeNamespaces, isRoot);

            allNodes.Add((namedType, root));

            // Recurse into child properties using the parameter name as prefix
            RecurseNodes(namedType, visited, root, allNodes, includeNamespaces);
            return;
        }

        // Otherwise emit a documentation entry for this property
        ITableRowNode tableRow = property.CreateTableRowNode(parameterName);
        node.Table.Body.TableRows.Add(tableRow);
    }
}
