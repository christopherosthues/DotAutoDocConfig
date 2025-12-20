using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TitleNode : ITitleNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTitle(this);

    public string Title { get; }
}
