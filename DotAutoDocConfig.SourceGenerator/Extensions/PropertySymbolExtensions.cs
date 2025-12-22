using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class PropertySymbolExtensions
{
    extension(IPropertySymbol propertySymbol)
    {
        public string GetDefaultValue()
        {
            try
            {
                SyntaxReference? declRef = propertySymbol.DeclaringSyntaxReferences.FirstOrDefault();
                if (declRef != null)
                {
                    SyntaxNode node = declRef.GetSyntax();
                    if (node is PropertyDeclarationSyntax propSyntax && propSyntax.Initializer != null)
                    {
                        return propSyntax.Initializer.Value.ToString();
                    }
                }

                if (propertySymbol.Type.TypeKind == TypeKind.Enum)
                {
                    if (propertySymbol.Type is INamedTypeSymbol enumType)
                    {
                        IFieldSymbol? first = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.ConstantValue != null);
                        return first?.Name ?? string.Empty;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public ITableRowNode CreateTableRowNode(string parameterName)
        {
            ITableRowNode tableRow = new TableRowNode();
            tableRow.DataNodes.Add(new TableDataNode(parameterName));
            tableRow.DataNodes.Add(new TableDataNode(propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
            tableRow.DataNodes.Add(new TableDataNode(propertySymbol.GetDefaultValue()));
            tableRow.DataNodes.Add(new TableDataNode(string.IsNullOrEmpty(propertySymbol.GetExampleFromXml())
                ? propertySymbol.Type.GetExampleValue()
                : propertySymbol.GetExampleFromXml()));
            tableRow.DataNodes.Add(new TableDataNode(propertySymbol.GetSummary()));

            return tableRow;
        }

        public ITableRowNode CreateTableRowNodeWithLink(string parameterName)
        {
            ITableRowNode tableRow = new TableRowNode();
            tableRow.DataNodes.Add(new TableDataNode(parameterName));
            // TODO: file link for complex types
            tableRow.DataNodes.Add(new TableDataNode(propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
            tableRow.DataNodes.Add(new TableDataNode(propertySymbol.GetDefaultValue()));
            tableRow.DataNodes.Add(new TableDataNode(string.IsNullOrEmpty(propertySymbol.GetExampleFromXml())
                ? propertySymbol.Type.GetExampleValue()
                : propertySymbol.GetExampleFromXml()));
            tableRow.DataNodes.Add(new TableDataNode(propertySymbol.GetSummary()));
            return tableRow;
        }
    }
}
