using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class LinkNode(string href, string text) : ILinkNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderLink(this);

    public string Href { get; } = href;
    public string Content { get; } = text;
}
