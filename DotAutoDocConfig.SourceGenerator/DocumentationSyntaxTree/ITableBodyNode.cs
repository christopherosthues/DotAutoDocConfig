using System.Collections.Generic;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITableBodyNode : INode
{
    IList<ITableRowNode> TableRows { get; }
}
