using System.Collections.Generic;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITableRowNode : INode
{
    IList<ITableDataNode> DataNodes { get; }
}
