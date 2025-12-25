using System.Text;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal class AsciiDocDocumentationRenderer : IDocumentationRenderer
{
    private readonly StringBuilder _builder = new();

    public void RenderComment(string comment) => _builder.Append("// ").AppendLine(comment);

    public void RenderLineBreak() => _builder.AppendLine();

    public void RenderTitle(ITitleNode node) => _builder.AppendLine($"= {EscapeAsciiDoc(node.Content)}").AppendLine();

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

    public void RenderTableHeader(ITableHeaderNode node) => node.TableHeaderRow.Accept(this);

    public void RenderTableHeaderRow(ITableHeaderRowNode node)
    {
        _builder.AppendLine("[options=\"header\"]");
        _builder.AppendLine("|===");
        foreach (ITableHeaderDataNode dataNode in node.TableHeaderDataNodes)
        {
            dataNode.Accept(this);
        }
        _builder.AppendLine().AppendLine();
    }

    public void RenderTableHeaderData(ITableHeaderDataNode node) =>
        _builder.Append("| ").Append(EscapeAsciiDoc(node.Content)).Append(" ");

    public void RenderTableBody(ITableBodyNode node)
    {
        foreach (ITableRowNode tableRowNode in node.TableRows)
        {
            tableRowNode.Accept(this);
        }
        _builder.AppendLine("|===");
    }

    public void RenderTableBodyRow(ITableRowNode node)
    {
        foreach (ITableDataNode dataNode in node.DataNodes)
        {
            dataNode.Accept(this);
        }
        _builder.AppendLine();
    }

    public void RenderTableData(ITableDataNode node)
    {
        _builder.Append("| ");
        node.Content.Accept(this);
        _builder.AppendLine();
    }

    public string GetResult() => _builder.ToString();

    public void Clear() => _builder.Clear();

    private static string EscapeAsciiDoc(string input)
    {
        return input
            .Replace("|", "\\|")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }

    public void RenderLink(ILinkNode node) => _builder.Append("xref:").Append(node.Href).Append("[")
        .Append(EscapeAsciiDoc(node.Content)).Append("]");

    public void RenderText(ITextNode node) => _builder.Append(EscapeAsciiDoc(node.Content));

    // private static string LinkToFile(string text, INamedTypeSymbol target, Dictionary<INamedTypeSymbol, string> typeToFileName)
    // {
    //     string name = row.ComplexTarget is null
    //         ? model.ParameterName
    //         : LinkToFile(model.ParameterName, row.ComplexTarget!, typeToFileName);
    //
    //     string fileName = typeToFileName[target];
    //     return $"xref:{fileName}[{text}]";
    // }
}
