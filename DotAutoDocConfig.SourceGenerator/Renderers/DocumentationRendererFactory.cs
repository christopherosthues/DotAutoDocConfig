using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.Renderers;

internal static class DocumentationRendererFactory
{
    public static IDocumentationRenderer CreateRenderer(LocalFormat format)
    {
        return format switch
        {
            LocalFormat.AsciiDoc => new AsciiDocDocumentationRenderer(),
            LocalFormat.Markdown => new MarkdownDocumentationRenderer(),
            LocalFormat.Html => new HtmlDocumentationRenderer(),
            _ => new AsciiDocDocumentationRenderer(),
        };
    }
}
