using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class SummaryNode : ISummaryNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderSummary(this);

    public string Summary { get; }
}
