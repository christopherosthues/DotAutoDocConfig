using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

public static class MarkdownGenerator
{
    public static void GenerateMarkdown(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries)
    {
        sb.AppendLine($"# Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"## {FriendlyQualifiedName(classSymbol)}");
        sb.AppendLine();

        // Table header (Markdown)
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description |");
        sb.AppendLine("|---|---|---|---|---|");

        foreach (DocumentationDataModel? e in entries)
        {
            sb.AppendLine($"| {EscapePipe(e.ParameterName)} | {EscapePipe(e.ParameterType)} | {EscapePipe(e.DefaultValue)} | {EscapePipe(e.ExampleValue)} | {EscapePipe(e.Summary)} |");
        }

        sb.AppendLine();
    }

    public static void GenerateMarkdownTables(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables)
    {
        sb.AppendLine($"# Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"## {FriendlyQualifiedName(classSymbol)}");
        sb.AppendLine();

        // Root table
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (TableRow row in tables.RootRows)
        {
            string name = row.ComplexTarget is null
                ? row.Data.ParameterName
                : LinkTo(row.Data.ParameterName, row.ComplexTarget);
            sb.AppendLine($"| {EscapePipe(name)} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)} |");
        }
        sb.AppendLine();

        // Sub tables (unique per type)
        foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
        {
            string anchor = AnchorFor(kvp.Key);
            sb.AppendLine($"### {FriendlyQualifiedName(kvp.Key)}");
            sb.AppendLine($"<a id=\"{anchor}\"></a>");
            sb.AppendLine();
            sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (TableRow row in kvp.Value)
            {
                string name = row.ComplexTarget is null
                    ? row.Data.ParameterName
                    : LinkTo(row.Data.ParameterName, row.ComplexTarget);
                sb.AppendLine($"| {EscapePipe(name)} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)} |");
            }
            sb.AppendLine();
        }
    }

    // New: Root-only renderer that links complex properties to separate files.
    public static void GenerateMarkdownRootWithFileLinks(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        sb.AppendLine($"# Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"## {FriendlyQualifiedName(classSymbol)}");
        sb.AppendLine();

        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (TableRow row in tables.RootRows)
        {
            string name = row.ComplexTarget is null
                ? row.Data.ParameterName
                : LinkToFile(row.Data.ParameterName, row.ComplexTarget!, typeToFileName);
            sb.AppendLine($"| {EscapePipe(name)} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)} |");
        }
        sb.AppendLine();
    }

    // New: Single-type table renderer (used for separate files)
    public static void GenerateMarkdownTypeTable(StringBuilder sb, INamedTypeSymbol typeSymbol, List<TableRow> rows)
    {
        sb.AppendLine($"# {FriendlyQualifiedName(typeSymbol)} Configuration");
        sb.AppendLine();
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (TableRow row in rows)
        {
            string name = row.Data.ParameterName;
            sb.AppendLine($"| {EscapePipe(name)} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)} |");
        }
        sb.AppendLine();
    }

    private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        string fileName = typeToFileName[target];
        return $"[{text}]({fileName})";
    }

    private static string AnchorFor(INamedTypeSymbol symbol)
    {
        // anchor based on fully qualified metadata name for uniqueness
        string fq = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        // sanitize to alnum and dashes
        StringBuilder sb = new();
        foreach (char c in fq)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else if (c == '.' || c == '_')
            {
                sb.Append('-');
            }
        }
        return sb.ToString();
    }

    private static string LinkTo(string text, INamedTypeSymbol target)
    {
        string anchor = AnchorFor(target);
        return $"[{text}](#{anchor})";
    }

    private static string EscapePipe(string? input)
    {
        string s = input ?? string.Empty;
        return s.Replace("|", "\\|");
    }

    private static string FriendlyQualifiedName(INamedTypeSymbol symbol)
    {
        string ns = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (string.IsNullOrEmpty(ns)) return symbol.Name;
        return ns + "." + symbol.Name;
    }
}
