using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal class HtmlDocumentationRenderer : IDocumentationRenderer
{
    public void RenderTitle(ITitleNode node) => throw new System.NotImplementedException();

    public void RenderSubtitle(ISubtitleNode node) => throw new System.NotImplementedException();

    public void RenderSummary(ISummaryNode node) => throw new System.NotImplementedException();

    public void RenderTable(ITableNode node) => throw new System.NotImplementedException();

    public void RenderTableHeader(ITableHeaderNode node) => throw new System.NotImplementedException();

    public void RenderTableHeaderRow(ITableHeaderRowNode node) => throw new System.NotImplementedException();

    public void RenderTableHeaderData(ITableHeaderDataNode node) => throw new System.NotImplementedException();

    public void RenderTableBody(ITableBodyNode node) => throw new System.NotImplementedException();

    public void RenderTableBodyRow(ITableRowNode node) => throw new System.NotImplementedException();

    public void RenderTableData(ITableDataNode node) => throw new System.NotImplementedException();

    public string GetResult() => throw new System.NotImplementedException();

    public void Clear() => throw new System.NotImplementedException();
}
