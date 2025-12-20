using System.Collections.Generic;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITableHeaderRowNode : INode
{
    IList<ITableHeaderDataNode> TableHeaderDataNodes { get; }
}
