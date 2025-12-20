namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITableHeaderNode : INode
{
    ITableHeaderRowNode TableHeaderRow { get; }
}
