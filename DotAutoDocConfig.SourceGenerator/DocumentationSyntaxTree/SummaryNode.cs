using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class SummaryNode(string content) : ISummaryNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderSummary(this);

    public string Content { get; } = content;
}
