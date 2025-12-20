namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITableNode : INode
{
    ITableHeaderNode Header { get; }
    ITableBodyNode Body { get; }
}
