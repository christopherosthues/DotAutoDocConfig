namespace DotAutoDocConfig.SourceGenerator.Models;

internal class DocumentationOptionsDataModel
{
    // Use byte for format to avoid referencing the external DocumentationFormat enum from DotAutoDocConfig.Core
    public LocalFormat Format { get; set; }

    // Use byte to avoid referencing ComplexParameterFormat from the Core project
    public ComplexParameterFormat ComplexParameterFormat { get; set; }

    public string OutputDirectory { get; set; } = null!;

    // New: whether to include namespaces in headings/labels
    public bool IncludeNamespaces { get; set; }

    public override string ToString() =>
        $"Format={Format}, " +
        $"ComplexParameterFormat={ComplexParameterFormat}, " +
        $"IncludeNamespaces={IncludeNamespaces}";
}
