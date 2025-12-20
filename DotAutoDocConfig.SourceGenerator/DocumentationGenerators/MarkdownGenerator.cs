using System.Collections.Generic;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

internal class MarkdownGenerator : IDocumentationGenerator
{
    public void Generate(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries, bool includeNamespaces)
    {
        GenerateRootTableHeader(sb, classSymbol, includeNamespaces);

        foreach (DocumentationDataModel model in entries)
        {
            GenerateTableRow(sb, Escape(model.ParameterName), Escape(model.ParameterType), Escape(model.DefaultValue), Escape(model.ExampleValue), Escape(model.Summary));
        }

        sb.AppendLine();
    }

    public void GenerateWithFileLinks(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables, Dictionary<INamedTypeSymbol, string> typeToFileName, bool includeNamespaces)
    {
        GenerateRootTableHeader(sb, classSymbol, includeNamespaces);

        foreach (TableRow row in tables.RootRows)
        {
            DocumentationDataModel model = row.Data;
            string name = row.ComplexTarget is null
                ? model.ParameterName
                : LinkToFile(model.ParameterName, row.ComplexTarget!, typeToFileName);
            GenerateTableRow(sb, Escape(name), Escape(model.ParameterType), Escape(model.DefaultValue), Escape(model.ExampleValue), Escape(model.Summary));
        }
        sb.AppendLine();
    }

    public void GenerateTypeTable(StringBuilder sb, INamedTypeSymbol typeSymbol, List<TableRow> rows, Dictionary<INamedTypeSymbol, string> typeToFileName, bool includeNamespaces)
    {
        GenerateTitle(sb, typeSymbol.FriendlyQualifiedName(includeNamespaces));
        GenerateSummary(sb, typeSymbol);
        GenerateTableHeader(sb);
        foreach (TableRow row in rows)
        {
            DocumentationDataModel model = row.Data;
            string name = row.ComplexTarget is null
                ? model.ParameterName
                : LinkToFile(model.ParameterName, row.ComplexTarget!, typeToFileName);
            GenerateTableRow(sb, Escape(name), Escape(model.ParameterType), Escape(model.DefaultValue), Escape(model.ExampleValue), Escape(model.Summary));
        }
        sb.AppendLine();
    }

    private static void GenerateRootTableHeader(StringBuilder sb, INamedTypeSymbol classSymbol, bool includeNamespaces)
    {
        GenerateTitle(sb, "Configuration Documentation");
        sb.AppendLine();
        GenerateSubtitle(sb, classSymbol.FriendlyQualifiedName(includeNamespaces));

        GenerateSummary(sb, classSymbol);
        GenerateTableHeader(sb);
    }

    private static void GenerateTitle(StringBuilder sb, string title) => sb.AppendLine($"# {title}");

    private static void GenerateSubtitle(StringBuilder sb, string subtitle) => sb.AppendLine($"## {subtitle}");

    private static void GenerateTableHeader(StringBuilder sb)
    {
        GenerateTableRow(sb, "Parameter", "Type", "Default Value", "Example Value", "Description");
        GenerateTableRow(sb, "---", "---", "---", "---", "---");
    }

    private static void GenerateTableRow(StringBuilder sb, string parameter, string type, string defaultValue, string exampleValue, string summary)
    {
        sb.Append("|");
        sb.Append($" {parameter} |");
        sb.Append($" {type} |");
        sb.Append($" {defaultValue} |");
        sb.Append($" {exampleValue} |");
        sb.Append($" {summary} |");
        sb.AppendLine();
    }

    private static void GenerateSummary(StringBuilder sb, INamedTypeSymbol classSymbol)
    {
        string summary = classSymbol.GetSummary();
        if (!string.IsNullOrEmpty(summary))
        {
            sb.AppendLine();
            sb.AppendLine(summary);
        }
        sb.AppendLine();
    }

    private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        string fileName = typeToFileName[target];
        return $"[{text}]({fileName})";
    }

    private static string Escape(string? input)
    {
        string s = input ?? string.Empty;
        return s.Replace("|", "\\|");
    }
}
