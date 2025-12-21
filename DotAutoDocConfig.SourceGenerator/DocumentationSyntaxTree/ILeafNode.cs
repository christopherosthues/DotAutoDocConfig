namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ILeafNode : INode
{
    string Content { get; }
}
