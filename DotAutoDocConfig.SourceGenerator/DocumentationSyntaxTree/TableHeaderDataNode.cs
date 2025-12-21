using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TableHeaderDataNode(string content) : ITableHeaderDataNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTableHeaderData(this);

    public string Content { get; } = content;
}
