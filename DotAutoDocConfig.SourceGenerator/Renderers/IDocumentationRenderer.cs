using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal interface IDocumentationRenderer
{
    void RenderTitle(ITitleNode node);
    void RenderSubtitle(ISubtitleNode node);
    void RenderSummary(ISummaryNode node);
    void RenderTable(ITableNode node);
    void RenderTableHeader(ITableHeaderNode node);
    void RenderTableHeaderRow(ITableHeaderRowNode node);
    void RenderTableHeaderData(ITableHeaderDataNode node);
    void RenderTableBody(ITableBodyNode node);
    void RenderTableBodyRow(ITableRowNode node);
    void RenderTableData(ITableDataNode node);
    string GetResult();
    void Clear();
}
