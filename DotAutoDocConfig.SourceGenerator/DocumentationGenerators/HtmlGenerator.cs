using System.Collections.Generic;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

internal class HtmlGenerator : IDocumentationGenerator
{
    public void Generate(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries, bool includeNamespaces)
    {
        GenerateRootTableHeader(sb, classSymbol, includeNamespaces);

        sb.AppendLine("<tbody>");
        foreach (DocumentationDataModel? e in entries)
        {
            sb.AppendLine($"    <tr><td>{EscapeHtml(e.ParameterName)}</td><td>{EscapeHtml(e.ParameterType)}</td><td>{EscapeHtml(e.DefaultValue)}</td><td>{EscapeHtml(e.ExampleValue)}</td><td>{EscapeHtml(e.Summary)}</td></tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
    }

    public void GenerateWithFileLinks(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables, Dictionary<INamedTypeSymbol, string> typeToFileName, bool includeNamespaces)
    {
        GenerateRootTableHeader(sb, classSymbol, includeNamespaces);

        sb.AppendLine("<tbody>");
        foreach (TableRow row in tables.RootRows)
        {
            string name = row.ComplexTarget is null
                ? EscapeHtml(row.Data.ParameterName)
                : LinkToFile(row.Data.ParameterName, row.ComplexTarget!, typeToFileName);
            sb.AppendLine($"    <tr><td>{name}</td><td>{EscapeHtml(row.Data.ParameterType)}</td><td>{EscapeHtml(row.Data.DefaultValue)}</td><td>{EscapeHtml(row.Data.ExampleValue)}</td><td>{EscapeHtml(row.Data.Summary)}</td></tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
    }

    public void GenerateTypeTable(StringBuilder sb, INamedTypeSymbol typeSymbol, List<TableRow> rows, bool includeNamespaces)
    {
        sb.AppendLine($"<h1>{EscapeHtml(typeSymbol.FriendlyQualifiedName(includeNamespaces))} Configuration</h1>");
        GenerateSummary(sb, typeSymbol);
        GenerateTableHeader(sb);

        sb.AppendLine("<tbody>");
        foreach (TableRow row in rows)
        {
            string name = EscapeHtml(row.Data.ParameterName);
            sb.AppendLine($"    <tr><td>{name}</td><td>{EscapeHtml(row.Data.ParameterType)}</td><td>{EscapeHtml(row.Data.DefaultValue)}</td><td>{EscapeHtml(row.Data.ExampleValue)}</td><td>{EscapeHtml(row.Data.Summary)}</td></tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
    }

    private static void GenerateRootTableHeader(StringBuilder sb, INamedTypeSymbol classSymbol, bool includeNamespaces)
    {
        sb.AppendLine("<h1>Configuration Documentation</h1>");
        sb.AppendLine($"<h2>{EscapeHtml(classSymbol.FriendlyQualifiedName(includeNamespaces))}</h2>");
        GenerateSummary(sb, classSymbol);
        GenerateTableHeader(sb);
    }

    private static void GenerateTableHeader(StringBuilder sb)
    {
        sb.AppendLine("<table>");
        sb.AppendLine("<thead>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("        <th>Parameter</th>");
        sb.AppendLine("        <th>Type</th>");
        sb.AppendLine("        <th>Default Value</th>");
        sb.AppendLine("        <th>Example Value</th>");
        sb.AppendLine("        <th>Description</th>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("</thead>");
    }

    private static void GenerateSummary(StringBuilder sb, INamedTypeSymbol classSymbol)
    {
        string summary = classSymbol.GetSummary();
        sb.AppendLine($"<p>{EscapeHtml(summary)}</p>");
    }

    private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        string fileName = typeToFileName[target];
        return $"<a href=\"{fileName}\">{EscapeHtml(text)}</a>";
    }

    private static string EscapeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return System.Security.SecurityElement.Escape(input) ?? string.Empty;
    }
}
