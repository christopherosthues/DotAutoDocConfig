namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ILinkNode : ILeafNode
{
    public string Href { get; }
}
