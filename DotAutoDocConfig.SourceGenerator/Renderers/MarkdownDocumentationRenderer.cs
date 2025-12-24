using System.Text;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal class MarkdownDocumentationRenderer : IDocumentationRenderer
{
    private readonly StringBuilder _builder = new();

    public void RenderComment(string comment) => _builder.Append("[//]: # (").Append(comment).AppendLine(")");

    public void RenderLineBreak() => _builder.AppendLine();

    public void RenderTitle(ITitleNode node) => _builder.AppendLine($"# {EscapeMarkdown(node.Content)}").AppendLine();

    public void RenderSummary(ISummaryNode node)
    {
        if (!string.IsNullOrEmpty(node.Content))
        {
            _builder.AppendLine(node.Content).AppendLine();
        }
    }

    public void RenderTable(ITableNode node)
    {
        node.Header.Accept(this);
        node.Body.Accept(this);
    }

    public void RenderTableHeader(ITableHeaderNode node)
    {
        node.TableHeaderRow.Accept(this);
        RenderTableHeaderSeparator(node.TableHeaderRow);
    }

    public void RenderTableHeaderRow(ITableHeaderRowNode node)
    {
        if (node.TableHeaderDataNodes.Count > 0)
        {
            _builder.Append("|");
        }
        foreach (ITableHeaderDataNode dataNode in node.TableHeaderDataNodes)
        {
            dataNode.Accept(this);
        }
        _builder.AppendLine();
    }

    private void RenderTableHeaderSeparator(ITableHeaderRowNode node)
    {
        if (node.TableHeaderDataNodes.Count > 0)
        {
            _builder.Append("| ");
        }
        foreach (ITableHeaderDataNode _ in node.TableHeaderDataNodes)
        {
            _builder.Append("--- |");
        }

        _builder.AppendLine();
    }

    public void RenderTableHeaderData(ITableHeaderDataNode node) =>
        _builder.Append(" ").Append(EscapeMarkdown(node.Content)).Append(" |");

    public void RenderTableBody(ITableBodyNode node)
    {
        foreach (ITableRowNode tableRowNode in node.TableRows)
        {
            tableRowNode.Accept(this);
        }
    }

    public void RenderTableBodyRow(ITableRowNode node)
    {
        if (node.DataNodes.Count > 0)
        {
            _builder.Append("|");
        }
        foreach (ITableDataNode dataNode in node.DataNodes)
        {
            dataNode.Accept(this);
        }
        _builder.AppendLine();
    }

    public void RenderTableData(ITableDataNode node) =>
        _builder.Append(" ").Append(EscapeMarkdown(node.Content)).Append(" |");

    public string GetResult() => _builder.ToString();

    public void Clear() => _builder.Clear();

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

    // private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    // {
    //     string name = row.ComplexTarget is null
    //         ? model.ParameterName
    //         : LinkToFile(model.ParameterName, row.ComplexTarget!, typeToFileName);
    //
    //     string fileName = typeToFileName[target];
    //     return $"[{text}]({fileName})";
    // }
}
