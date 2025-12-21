using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface IDocumentationNode : INode
{
    INamedTypeSymbol Type { get; }
    ITitleNode Title { get; }
    ISubtitleNode? Subtitle { get; }
    ISummaryNode Summary { get; }
    ITableNode Table { get; }
}
