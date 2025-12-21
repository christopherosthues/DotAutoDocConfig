using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class SubtitleNode(string content) : ISubtitleNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderSubtitle(this);

    public string Content { get; } = content;
}
