using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

internal static class DocumentationGeneratorFactory
{
    public static IDocumentationGenerator CreateGenerator(LocalFormat format)
    {
        return format switch
        {
            LocalFormat.Markdown => new MarkdownGenerator(),
            LocalFormat.AsciiDoc => new AsciiDocGenerator(),
            _ => new MarkdownGenerator()
        };
    }
}
