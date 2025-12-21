using System.Collections.Generic;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

internal class HtmlGenerator : IDocumentationGenerator
{
    // TODO: Delete me
    public void Generate(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries, bool includeNamespaces)
    {
        GenerateRootTableHeader(sb, classSymbol, includeNamespaces);

        sb.AppendLine("<tbody>");
        foreach (DocumentationDataModel? e in entries)
        {
            sb.AppendLine($"    <tr><td>{Escape(e.ParameterName)}</td><td>{Escape(e.ParameterType)}</td><td>{Escape(e.DefaultValue)}</td><td>{Escape(e.ExampleValue)}</td><td>{Escape(e.Summary)}</td></tr>");
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
            DocumentationDataModel model = row.Data;
            string name = row.ComplexTarget is null
                ? Escape(model.ParameterName)
                : LinkToFile(model.ParameterName, row.ComplexTarget!, typeToFileName);
            sb.AppendLine($"    <tr><td>{name}</td><td>{Escape(model.ParameterType)}</td><td>{Escape(model.DefaultValue)}</td><td>{Escape(model.ExampleValue)}</td><td>{Escape(model.Summary)}</td></tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
    }

    public void GenerateTypeTable(StringBuilder sb, INamedTypeSymbol typeSymbol, List<TableRow> rows, Dictionary<INamedTypeSymbol, string> typeToFileName, bool includeNamespaces)
    {
        sb.AppendLine($"<h1>{Escape(typeSymbol.FriendlyQualifiedName(includeNamespaces))} Configuration</h1>");
        GenerateSummary(sb, typeSymbol);
        GenerateTableHeader(sb);

        sb.AppendLine("<tbody>");
        foreach (TableRow row in rows)
        {
            DocumentationDataModel model = row.Data;
            string name = row.ComplexTarget is null
                ? model.ParameterName
                : LinkToFile(model.ParameterName, row.ComplexTarget!, typeToFileName);
            sb.AppendLine($"    <tr><td>{name}</td><td>{Escape(model.ParameterType)}</td><td>{Escape(model.DefaultValue)}</td><td>{Escape(model.ExampleValue)}</td><td>{Escape(model.Summary)}</td></tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
    }

    private static void GenerateRootTableHeader(StringBuilder sb, INamedTypeSymbol classSymbol, bool includeNamespaces)
    {
        sb.AppendLine("<h1>Configuration Documentation</h1>");
        sb.AppendLine($"<h2>{Escape(classSymbol.FriendlyQualifiedName(includeNamespaces))}</h2>");
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
        sb.AppendLine($"<p>{Escape(summary)}</p>");
    }

    private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        string fileName = typeToFileName[target];
        return $"<a href=\"{fileName}\">{Escape(text)}</a>";
    }

    private static string Escape(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return System.Security.SecurityElement.Escape(input) ?? string.Empty;
    }
}
