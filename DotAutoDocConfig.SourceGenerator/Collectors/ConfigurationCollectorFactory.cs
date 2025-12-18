using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.Collectors;

internal static class ConfigurationCollectorFactory
{
    public static IConfigurationCollector CreateCollector(ComplexParameterFormat format)
    {
        return format switch
        {
            ComplexParameterFormat.InlineJsonShort => new InlineTableCollector(),
            ComplexParameterFormat.SeparateTables => new SeparateTableCollector(),
            _ => new InlineTableCollector()
        };
    }
}
