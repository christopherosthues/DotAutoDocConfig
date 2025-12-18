using System.Collections.Generic;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

internal class AsciiDocGenerator : IDocumentationGenerator
{
    public void Generate(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries, bool includeNamespaces)
    {
        GenerateRootTableHeader(sb, classSymbol, includeNamespaces);

        foreach (DocumentationDataModel? e in entries)
        {
            sb.AppendLine();
            sb.AppendLine($"| {e.ParameterName} | {EscapePipe(e.ParameterType)} | {EscapePipe(e.DefaultValue)} | {EscapePipe(e.ExampleValue)} | {EscapePipe(e.Summary)}");
        }

        sb.AppendLine("|===");
    }

    public void GenerateWithFileLinks(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables, Dictionary<INamedTypeSymbol, string> typeToFileName, bool includeNamespaces)
    {
        GenerateRootTableHeader(sb, classSymbol, includeNamespaces);

        foreach (TableRow row in tables.RootRows)
        {
            string name = row.ComplexTarget is null
                ? row.Data.ParameterName
                : LinkToFile(row.Data.ParameterName, row.ComplexTarget!, typeToFileName);
            sb.AppendLine();
            sb.AppendLine($"| {name} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)}");
        }
        sb.AppendLine("|===");
        sb.AppendLine();
    }

    public void GenerateTypeTable(StringBuilder sb, INamedTypeSymbol typeSymbol, List<TableRow> rows, bool includeNamespaces)
    {
        sb.AppendLine($"= {typeSymbol.FriendlyQualifiedName(includeNamespaces)} Configuration");
        GenerateSummary(sb, typeSymbol);
        GenerateTableHeader(sb);
        foreach (TableRow row in rows)
        {
            string name = row.Data.ParameterName;
            sb.AppendLine();
            sb.AppendLine($"| {name} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)}");
        }
        sb.AppendLine("|===");
    }

    private static void GenerateRootTableHeader(StringBuilder sb, INamedTypeSymbol classSymbol, bool includeNamespaces)
    {
        sb.AppendLine("= Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"== {classSymbol.FriendlyQualifiedName(includeNamespaces)}");

        GenerateSummary(sb, classSymbol);
        GenerateTableHeader(sb);
    }

    private static void GenerateTableHeader(StringBuilder sb)
    {
        sb.AppendLine("[options=\"header\"]");
        sb.AppendLine("|===");
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description");
    }

    private static void GenerateSummary(StringBuilder sb, INamedTypeSymbol classSymbol) => classSymbol.AddSummary(sb);

    private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        string fileName = typeToFileName[target];
        return $"xref:{fileName}[{text}]";
    }

    private static string EscapePipe(string? input)
    {
        string s = input ?? string.Empty;
        return s.Replace("|", "\\|");
    }


}
