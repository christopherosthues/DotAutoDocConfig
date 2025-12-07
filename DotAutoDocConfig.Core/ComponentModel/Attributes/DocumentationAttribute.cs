using System;

namespace DotAutoDocConfig.Core.ComponentModel.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class DocumentationAttribute(DocumentationFormat format, string outputPath) : Attribute
{
    public DocumentationFormat Format { get; } = format;

    public string OutputPath { get; set; } = outputPath;
}
