using System.Text;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal class HtmlDocumentationRenderer : IDocumentationRenderer
{
    private readonly StringBuilder _builder = new();

    public void RenderTitle(ITitleNode node)
    {

    }

    public void RenderSubtitle(ISubtitleNode node)
    {

    }

    public void RenderSummary(ISummaryNode node)
    {

    }

    public void RenderTable(ITableNode node)
    {

    }

    public void RenderTableHeader(ITableHeaderNode node)
    {

    }

    public void RenderTableHeaderRow(ITableHeaderRowNode node)
    {

    }

    public void RenderTableHeaderData(ITableHeaderDataNode node)
    {

    }

    public void RenderTableBody(ITableBodyNode node)
    {

    }

    public void RenderTableBodyRow(ITableRowNode node)
    {

    }

    public void RenderTableData(ITableDataNode node)
    {

    }

    public string GetResult() => _builder.ToString();

    public void Clear() => _builder.Clear();
}
