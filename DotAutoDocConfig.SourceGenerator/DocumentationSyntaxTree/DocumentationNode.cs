using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal class DocumentationNode : IDocumentationNode
{
    public void Accept(IDocumentationRenderer renderer)
    {
        Title.Accept(renderer);
        Subtitle?.Accept(renderer);
        Summary?.Accept(renderer);
        Table.Accept(renderer);
    }

    public ITitleNode Title { get; set; } = new TitleNode(string.Empty);
    public ISubtitleNode? Subtitle { get; set; }
    public ISummaryNode? Summary { get; set; }
    public ITableNode Table { get; set; } = new TableNode();
}
