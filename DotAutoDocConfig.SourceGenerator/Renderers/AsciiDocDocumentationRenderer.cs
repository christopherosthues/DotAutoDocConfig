using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal class AsciiDocDocumentationRenderer : IDocumentationRenderer
{
    public void RenderTitle(ITitleNode node) => throw new System.NotImplementedException();

    public void RenderSubtitle(ISubtitleNode node) => throw new System.NotImplementedException();

    public void RenderSummary(ISummaryNode node) => throw new System.NotImplementedException();

    public void RenderTable(ITableNode node) => throw new System.NotImplementedException();

    public string GetResult() => throw new System.NotImplementedException();
}
