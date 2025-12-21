using System.Collections.Generic;
using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TableRowNode : ITableRowNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTableBodyRow(this);
    public IList<ITableDataNode> DataNodes { get; } = [];
}
