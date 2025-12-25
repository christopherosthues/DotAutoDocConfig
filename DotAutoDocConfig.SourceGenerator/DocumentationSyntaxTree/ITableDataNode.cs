namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITableDataNode : INode
{
    public ILeafNode Content { get; }
}
