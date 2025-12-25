using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface IDocumentationNode : INode
{
    INamedTypeSymbol NamedTypeSymbol { get; }
    string OutputFilePath { get; }
    ITitleNode Title { get; }
    ISummaryNode? Summary { get; }
    ITableNode Table { get; }
}
