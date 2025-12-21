using System.Collections.Generic;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal class SeparateTableParser : IDocumentationParser
{
    public IList<IDocumentationNode> Parse(INamedTypeSymbol namedTypeSymbol, bool includeNamespaces)
    {
        List<IDocumentationNode> allNodes = [];
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default);

        DocumentationNode root = new()
        {
            Title = new TitleNode("Configuration Documentation"),
            Subtitle = new SubtitleNode(namedTypeSymbol.FriendlyQualifiedName(includeNamespaces))
        };

        string summaryContent = namedTypeSymbol.GetSummary();
        if (string.IsNullOrEmpty(summaryContent))
        {
            root.Summary = new SummaryNode(summaryContent);
        }

        root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Parameter Name"));
        root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Parameter Type"));
        root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Default Value"));
        root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Example Value"));
        root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Description"));

        allNodes.Add(root);

        RecurseNodes(namedTypeSymbol, visited, root, allNodes, includeNamespaces);

        return allNodes;
    }

    private static void RecurseNodes(INamedTypeSymbol? current, HashSet<INamedTypeSymbol> visited,
        IDocumentationNode node, IList<IDocumentationNode> allNodes,
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
        IDocumentationNode node, IPropertySymbol property, IList<IDocumentationNode> allNodes, bool includeNamespaces)
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
            DocumentationNode root = new()
            {
                Title = new TitleNode(namedType.FriendlyQualifiedName(includeNamespaces))
            };

            string summaryContent = namedType.GetSummary();
            if (string.IsNullOrEmpty(summaryContent))
            {
                root.Summary = new SummaryNode(summaryContent);
            }

            root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Parameter Name"));
            root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Parameter Type"));
            root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Default Value"));
            root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Example Value"));
            root.Table.Header.TableHeaderRow.TableHeaderDataNodes.Add(new TableHeaderDataNode("Description"));

            allNodes.Add(root);

            // Recurse into child properties using the parameter name as prefix
            RecurseNodes(namedType, visited, root, allNodes, includeNamespaces);
            return;
        }

        // Otherwise emit a documentation entry for this property
        ITableRowNode tableRow = new TableRowNode();
        tableRow.DataNodes.Add(new TableDataNode(parameterName));
        tableRow.DataNodes.Add(new TableDataNode(property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        tableRow.DataNodes.Add(new TableDataNode(property.GetDefaultValue()));
        tableRow.DataNodes.Add(new TableDataNode(property.GetSummary()));
        tableRow.DataNodes.Add(new TableDataNode(string.IsNullOrEmpty(property.GetExampleFromXml())
            ? property.Type.GetExampleValue()
            : property.GetExampleFromXml()));
    }
}
