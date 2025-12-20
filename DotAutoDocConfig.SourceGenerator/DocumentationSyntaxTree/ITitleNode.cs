namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ITitleNode : ILeafNode
{
    string Title { get; }
}
