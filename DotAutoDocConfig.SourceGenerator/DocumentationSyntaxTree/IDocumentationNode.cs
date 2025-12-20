namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface IDocumentationNode : INode
{
    ITitleNode Title { get; }
    ISubtitleNode? Subtitle { get; }
    ISummaryNode Summary { get; }
    ITableNode Table { get; }
}
