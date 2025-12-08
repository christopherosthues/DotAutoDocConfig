using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

public static class MarkdownGenerator
{
    public static void Generate(StringBuilder sb, INamedTypeSymbol classSymbol, IEnumerable<DocumentationDataModel> entries)
    {
        sb.AppendLine($"# Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"## {classSymbol.Name}");
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

    private static string EscapePipe(string? input)
    {
        string s = input ?? string.Empty;
        return s.Replace("|", "\\|");
    }
}
