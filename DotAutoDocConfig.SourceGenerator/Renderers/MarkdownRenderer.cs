using System.Text;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal class MarkdownRenderer : IDocumentationRenderer
{
    private readonly StringBuilder _builder = new();

    public void RenderTitle(ITitleNode node) => _builder.AppendLine($"# {EscapeMarkdown(node.Title)}");

    public void RenderSubtitle(ISubtitleNode node) => _builder.AppendLine($"## {EscapeMarkdown(node.Subtitle)}");

    public void RenderSummary(ISummaryNode node)
    {
        if (!string.IsNullOrEmpty(node.Summary))
        {
            _builder.AppendLine();
            _builder.AppendLine(node.Summary);
        }
        _builder.AppendLine();
    }

    public void RenderTable(ITableNode node)
    {
        throw new System.NotImplementedException();
    }

    public string GetResult() => _builder.ToString();

    private static string EscapeMarkdown(string input)
    {
        return input
            .Replace("\\", @"\\")
            .Replace("`", "\\`")
            .Replace("*", "\\*")
            .Replace("_", "\\_")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("#", "\\#")
            .Replace("+", "\\+")
            .Replace("-", "\\-")
            .Replace("!", "\\!")
            .Replace("|", "\\|");
    }
}
