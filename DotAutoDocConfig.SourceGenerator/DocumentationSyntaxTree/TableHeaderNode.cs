using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TableHeaderNode : ITableHeaderNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTableHeader(this);

    public ITableHeaderRowNode TableHeaderRow { get; } = new TableHeaderRowNode();
}
