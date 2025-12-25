using System.Text;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal class HtmlDocumentationRenderer : IDocumentationRenderer
{
    private readonly StringBuilder _builder = new();

    public void RenderComment(string comment) =>
        _builder.Append("<!-- ").Append(EscapeHtml(comment)).AppendLine(" -->");

    public void RenderLineBreak() => _builder.AppendLine();

    public void RenderTitle(ITitleNode node) =>
        _builder.Append("<h1>").Append(EscapeHtml(node.Content)).AppendLine("</h1>").AppendLine();

    public void RenderSummary(ISummaryNode node)
    {
        if (!string.IsNullOrEmpty(node.Content))
        {
            _builder.Append("<p>")
                .Append(EscapeHtml(node.Content))
                .AppendLine("</p>")
                .AppendLine();
        }
    }

    public void RenderTable(ITableNode node)
    {
        _builder.AppendLine("<table>");
        node.Header.Accept(this);
        node.Body.Accept(this);
        _builder.AppendLine("</table>").AppendLine();
    }

    public void RenderTableHeader(ITableHeaderNode node)
    {
        _builder.AppendLine("<thead>");
        node.TableHeaderRow.Accept(this);
        _builder.AppendLine("</thead>");
    }

    public void RenderTableHeaderRow(ITableHeaderRowNode node)
    {
        _builder.AppendLine("<tr>");
        foreach (ITableHeaderDataNode dataNode in node.TableHeaderDataNodes)
        {
            dataNode.Accept(this);
        }
        _builder.AppendLine("</tr>");
    }

    public void RenderTableHeaderData(ITableHeaderDataNode node)
    {
        _builder.Append("<th>")
            .Append(EscapeHtml(node.Content))
            .AppendLine("</th>");
    }

    public void RenderTableBody(ITableBodyNode node)
    {
        _builder.AppendLine("<tbody>");
        foreach (ITableRowNode tableRowNode in node.TableRows)
        {
            tableRowNode.Accept(this);
        }
        _builder.AppendLine("</tbody>");
    }

    public void RenderTableBodyRow(ITableRowNode node)
    {
        _builder.AppendLine("<tr>");
        foreach (ITableDataNode dataNode in node.DataNodes)
        {
            dataNode.Accept(this);
        }
        _builder.AppendLine("</tr>");
    }

    public void RenderTableData(ITableDataNode node)
    {
        _builder.Append("<td>");
        node.Content.Accept(this);
        _builder.AppendLine("</td>");
    }

    public string GetResult() => _builder.ToString();

    public void Clear() => _builder.Clear();

    private static string EscapeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return System.Net.WebUtility.HtmlEncode(input) ?? string.Empty;
    }

    public void RenderLink(ILinkNode node) =>
        _builder.Append("<a href=\"")
            .Append(EscapeHtml(node.Href))
            .Append("\">")
            .Append(EscapeHtml(node.Content))
            .Append("</a>");

    public void RenderText(ITextNode node) => _builder.Append(EscapeHtml(node.Content));
}
