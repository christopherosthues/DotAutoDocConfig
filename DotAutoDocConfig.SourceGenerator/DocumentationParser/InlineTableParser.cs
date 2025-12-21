using System.Collections.Generic;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal class InlineTableParser : IDocumentationParser
{
    public IList<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> Parse(INamedTypeSymbol namedTypeSymbol, bool includeNamespaces)
    {
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

        RecurseNodes(namedTypeSymbol, string.Empty, visited, root);

        return [(namedTypeSymbol, root)];
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
        ITableRowNode tableRow = new TableRowNode();
        tableRow.DataNodes.Add(new TableDataNode(parameterName));
        tableRow.DataNodes.Add(new TableDataNode(property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        tableRow.DataNodes.Add(new TableDataNode(property.GetDefaultValue()));
        tableRow.DataNodes.Add(new TableDataNode(string.IsNullOrEmpty(property.GetExampleFromXml())
            ? property.Type.GetExampleValue()
            : property.GetExampleFromXml()));
        tableRow.DataNodes.Add(new TableDataNode(property.GetSummary()));
    }
}
