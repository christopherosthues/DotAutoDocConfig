using DotAutoDocConfig.Core.ComponentModel;

namespace DotAutoDocConfig.SourceGenerator.Models;

public class DocumentationOptionsDataModel
{
    public DocumentationFormat Format { get; set; }

    public string OutputPath { get; set; } = null!;
}
