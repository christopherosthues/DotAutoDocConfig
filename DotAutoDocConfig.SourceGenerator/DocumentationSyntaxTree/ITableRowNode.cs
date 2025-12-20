using System.Collections.Generic;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITableRowNode
{
    IList<ITableDataNode> DataNodes { get; }
}
