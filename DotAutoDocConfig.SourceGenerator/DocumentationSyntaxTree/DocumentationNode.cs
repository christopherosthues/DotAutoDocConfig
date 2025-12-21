using DotAutoDocConfig.SourceGenerator.Renderers;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class DocumentationNode : IDocumentationNode
{
    public void Accept(IDocumentationRenderer renderer)
    {
        Title.Accept(renderer);
        if (Subtitle is not null)
        {
            Subtitle.Accept(renderer);
        }
        Summary.Accept(renderer);
        Table.Accept(renderer);
    }

    public INamedTypeSymbol Type { get; }
    public ITitleNode Title { get; set; }
    public ISubtitleNode? Subtitle { get; set; }
    public ISummaryNode Summary { get; set; }
    public ITableNode Table { get; set; } = new TableNode();
}
