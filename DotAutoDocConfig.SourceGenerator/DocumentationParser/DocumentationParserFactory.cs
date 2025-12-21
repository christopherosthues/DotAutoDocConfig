using DotAutoDocConfig.SourceGenerator.Collectors;
using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal class DocumentationParserFactory
{
    public static IDocumentationParser CreateParser(ComplexParameterFormat complexParameterFormat)
    {
        return complexParameterFormat switch
        {
            ComplexParameterFormat.InlineJsonShort => new InlineTableParser(),
            ComplexParameterFormat.SeparateTables => new SeparateTableParser(),
            _ => new InlineTableParser(),
        };
    }
}
