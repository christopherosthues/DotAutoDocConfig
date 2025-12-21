using System.Collections.Generic;
using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TableHeaderRowNode : ITableHeaderRowNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTableHeaderRow(this);

    public IList<ITableHeaderDataNode> TableHeaderDataNodes { get; } = [];
}
