using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TextNode(string content) : ITextNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderText(this);

    public string Content { get; } = content;
}
