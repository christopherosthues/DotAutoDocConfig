namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface ISummaryNode : ILeafNode
{
    string Summary { get; } // classSymbol.GetSummary()
}
