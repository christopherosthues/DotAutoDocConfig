using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Models;

public class DocumentationDataModel
{
    public INamedTypeSymbol ClassSymbol { get; set; } = null!;
    public string ParameterName { get; set; } = null!;
    public string ParameterType { get; set; } = null!;
    public string DefaultValue { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string ExampleValue { get; set; } = null!;
}
