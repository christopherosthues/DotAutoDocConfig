using DotAutoDocConfig.SourceGenerator.Renderers;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class DocumentationNode(INamedTypeSymbol namedTypeSymbol) : IDocumentationNode
{
    public void Accept(IDocumentationRenderer renderer)
    {
        Title.Accept(renderer);
        Summary?.Accept(renderer);
        Table.Accept(renderer);
    }

    public INamedTypeSymbol NamedTypeSymbol { get; set; } = namedTypeSymbol;
    public ITitleNode Title { get; set; } = new TitleNode(string.Empty);
    public ISummaryNode? Summary { get; set; }
    public ITableNode Table { get; set; } = new TableNode();
}
