using DotAutoDocConfig.SourceGenerator.Renderers;

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

    public ITitleNode Title { get; }
    public ISubtitleNode? Subtitle { get; }
    public ISummaryNode Summary { get; }
    public ITableNode Table { get; }
}
