using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

internal static class DocumentationGeneratorFactory
{
    public static IDocumentationGenerator CreateGenerator(LocalFormat format)
    {
        return format switch
        {
            LocalFormat.AsciiDoc => new AsciiDocGenerator(),
            LocalFormat.Markdown => new MarkdownGenerator(),
            _ => new AsciiDocGenerator()
        };
    }
}
