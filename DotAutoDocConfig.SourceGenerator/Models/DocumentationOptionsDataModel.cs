using System;

namespace DotAutoDocConfig.SourceGenerator.Models;

public class DocumentationOptionsDataModel
{
    // Use byte for format to avoid referencing the external DocumentationFormat enum from DotAutoDocConfig.Core
    public byte Format { get; set; }

    public string OutputPath { get; set; } = null!;
}
