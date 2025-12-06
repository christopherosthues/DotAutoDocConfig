using System;

namespace DotAutoDocConfig.SourceGenerator;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true)]
public class DocumentationAttribute(DocumentationFormat format) : System.Attribute
{
    public DocumentationFormat Format { get; } = format;
}

[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true)]
public class ExcludeFromDocumentationAttribute(string? reason = null) : System.Attribute
{
    public string? Reason { get; } = reason;
}

/// <summary>
///
/// </summary>
/// <example></example>
[Flags]
public enum DocumentationFormat : byte
{
    None = 0,
    AsciiDoc = 1 << 0,
    Markdown = 1 << 1,
    Html = 1 << 2
}
