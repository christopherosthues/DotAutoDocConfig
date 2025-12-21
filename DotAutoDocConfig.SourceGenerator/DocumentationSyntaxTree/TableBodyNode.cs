using System.Collections.Generic;
using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class TableBodyNode : ITableBodyNode
{
    public void Accept(IDocumentationRenderer renderer) => renderer.RenderTableBody(this);

    public IList<ITableRowNode> TableRows { get; } = [];
}
