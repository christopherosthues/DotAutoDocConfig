using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal class SeparateTableParser : IDocumentationParser
{
    public IList<IDocumentationNode> Parse(INamedTypeSymbol namedTypeSymbol, DocumentationOptionsDataModel options,
        string directory, ISet<string> filePaths)
    {
        List<IDocumentationNode> allNodes = [];
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default);

        // TODO: file path
        string ext = options.Format.ToFileExtension();
        string baseName = options.IncludeNamespaces ? namedTypeSymbol.CreateFileBaseNameWithNamespace() : namedTypeSymbol.Name;
        string fileName = baseName.EnsureUniqueFileName(ext, filePaths);
        string filePath = Path.Combine(directory, fileName);

        IDocumentationNode root = namedTypeSymbol.CreateDocumentationNode(options, filePath);

        filePaths.Add(filePath);
        allNodes.Add(root);

        RecurseNodes(namedTypeSymbol, visited, root, allNodes, options, directory, filePaths);

        return allNodes;
    }

    private static void RecurseNodes(INamedTypeSymbol? current, HashSet<INamedTypeSymbol> visited,
        IDocumentationNode node, IList<IDocumentationNode> allNodes, DocumentationOptionsDataModel options,
        string directory, ISet<string> filePaths)
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
            ParseProperty(visited, node, member, allNodes, options, directory, filePaths);
        }
    }

    private static void ParseProperty(HashSet<INamedTypeSymbol> visited,
        IDocumentationNode node, IPropertySymbol property,
        IList<IDocumentationNode> allNodes, DocumentationOptionsDataModel options,
        string directory, ISet<string> filePaths)
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
            // TODO: file path
            string ext = options.Format.ToFileExtension();
            string baseName = options.IncludeNamespaces ? namedType.CreateFileBaseNameWithNamespace() : namedType.Name;
            string fileName = baseName.EnsureUniqueFileName(ext, filePaths);
            string filePath = Path.Combine(directory, fileName);

            ITableRowNode customTableRow = property.CreateTableRowNodeWithLink(parameterName, fileName);
            node.Table.Body.TableRows.Add(customTableRow);

            IDocumentationNode root = namedType.CreateDocumentationNode(options, filePath);

            filePaths.Add(filePath);

            allNodes.Add(root);

            // Recurse into child properties using the parameter name as prefix
            RecurseNodes(namedType, visited, root, allNodes, options, directory, filePaths);
            return;
        }

        // Otherwise emit a documentation entry for this property
        ITableRowNode tableRow = property.CreateTableRowNode(parameterName);
        node.Table.Body.TableRows.Add(tableRow);
    }
}
