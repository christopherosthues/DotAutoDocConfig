using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal interface IDocumentationRenderer
{
    void RenderTitle(ITitleNode node);
    void RenderSubtitle(ISubtitleNode node);
    void RenderSummary(ISummaryNode node);
    void RenderTable(ITableNode node);
    string GetResult();
}
