using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TableNode : ITableNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTable(this);

    public ITableHeaderNode Header { get; }
    public ITableBodyNode Body { get; }
}
