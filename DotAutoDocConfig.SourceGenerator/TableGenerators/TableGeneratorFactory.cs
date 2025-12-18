using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.TableGenerators;

internal static class TableGeneratorFactory
{
    public static ITableGenerator CreateGenerator(ComplexParameterFormat complexParameterFormat)
    {
        return complexParameterFormat switch{
            ComplexParameterFormat.InlineJsonShort => new InlineTableGenerator(),
            ComplexParameterFormat.SeparateTables => new SeparateTableGenerator(),
            _ => new InlineTableGenerator()
        };
    }
}
