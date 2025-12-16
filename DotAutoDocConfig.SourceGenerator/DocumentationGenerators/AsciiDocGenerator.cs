using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

public static class AsciiDocGenerator
{
    public static void GenerateAsciiDoc(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries)
    {
        sb.AppendLine("= Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"== {FriendlyQualifiedName(classSymbol)}");
        sb.AppendLine();

        // Table header (AsciiDoc)
        sb.AppendLine("[options=\"header\"]");
        sb.AppendLine("|===");
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description");

        foreach (DocumentationDataModel? e in entries)
        {
            sb.AppendLine();
            sb.AppendLine($"| {e.ParameterName} | {EscapePipe(e.ParameterType)} | {EscapePipe(e.DefaultValue)} | {EscapePipe(e.ExampleValue)} | {EscapePipe(e.Summary)}");
        }

        sb.AppendLine("|===");
    }

    public static void GenerateAsciiDocTables(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables)
    {
        sb.AppendLine("= Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"== {FriendlyQualifiedName(classSymbol)}");
        sb.AppendLine();

        // Root table
        sb.AppendLine("[options=\"header\"]");
        sb.AppendLine("|===");
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description");
        foreach (TableRow row in tables.RootRows)
        {
            string name = row.ComplexTarget is null
                ? row.Data.ParameterName
                : LinkTo(row.Data.ParameterName, row.ComplexTarget);
            sb.AppendLine();
            sb.AppendLine($"| {name} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)}");
        }
        sb.AppendLine("|===");
        sb.AppendLine();

        // Sub tables
        foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
        {
            string anchor = AnchorFor(kvp.Key);
            sb.AppendLine($"=== {FriendlyQualifiedName(kvp.Key)}");
            sb.AppendLine($"anchor:{anchor}[]");
            sb.AppendLine();
            sb.AppendLine("[options=\"header\"]");
            sb.AppendLine("|===");
            sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description");
            foreach (TableRow row in kvp.Value)
            {
                string name = row.ComplexTarget is null
                    ? row.Data.ParameterName
                    : LinkTo(row.Data.ParameterName, row.ComplexTarget);
                sb.AppendLine();
                sb.AppendLine($"| {name} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)}");
            }
            sb.AppendLine("|===");
            sb.AppendLine();
        }
    }

    // New: Root-only renderer with file links
    public static void GenerateAsciiDocRootWithFileLinks(StringBuilder sb, INamedTypeSymbol classSymbol, DocumentationTablesModel tables, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        sb.AppendLine("= Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"== {FriendlyQualifiedName(classSymbol)}");
        sb.AppendLine();

        sb.AppendLine("[options=\"header\"]");
        sb.AppendLine("|===");
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description");
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

    // New: Single-type table for separate files
    public static void GenerateAsciiDocTypeTable(StringBuilder sb, INamedTypeSymbol typeSymbol, List<TableRow> rows)
    {
        sb.AppendLine($"= {FriendlyQualifiedName(typeSymbol)} Configuration");
        sb.AppendLine();
        sb.AppendLine("[options=\"header\"]");
        sb.AppendLine("|===");
        sb.AppendLine("| Parameter | Type | Default Value | Example Value | Description");
        foreach (TableRow row in rows)
        {
            string name = row.Data.ParameterName;
            sb.AppendLine();
            sb.AppendLine($"| {name} | {EscapePipe(row.Data.ParameterType)} | {EscapePipe(row.Data.DefaultValue)} | {EscapePipe(row.Data.ExampleValue)} | {EscapePipe(row.Data.Summary)}");
        }
        sb.AppendLine("|===");
    }

    private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    {
        string fileName = typeToFileName[target];
        return $"xref:{fileName}[{text}]";
    }

    private static string AnchorFor(INamedTypeSymbol symbol)
    {
        string fq = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        StringBuilder sbAnchor = new();
        foreach (char c in fq)
        {
            if (char.IsLetterOrDigit(c))
            {
                sbAnchor.Append(c);
            }
            else if (c == '.' || c == '_')
            {
                sbAnchor.Append('-');
            }
        }
        return sbAnchor.ToString();
    }

    private static string LinkTo(string text, INamedTypeSymbol target)
    {
        string anchor = AnchorFor(target);
        // AsciiDoc cross-reference
        return $"xref:#{anchor}[{text}]";
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
