using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class SubtitleNode : ISubtitleNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderSubtitle(this);

    public string Subtitle { get; }
}
