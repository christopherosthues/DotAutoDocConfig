using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TableDataNode(string content) : ITableDataNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTableData(this);

    public string Content { get; } = content;
}
