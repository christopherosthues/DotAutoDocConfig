using System.Collections.Generic;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

internal interface IDocumentationGenerator
{
    void Generate(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries, bool includeNamespaces);

    void GenerateWithFileLinks(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables,
        Dictionary<INamedTypeSymbol, string> typeToFileName, bool includeNamespaces);

    void GenerateTypeTable(StringBuilder sb, INamedTypeSymbol typeSymbol, List<TableRow> rows,
        Dictionary<INamedTypeSymbol, string> typeToFileName, bool includeNamespaces);
}
