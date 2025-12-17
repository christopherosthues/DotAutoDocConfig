namespace DotAutoDocConfig.Core.ComponentModel;

/// <summary>
/// Defines the output format for generated configuration documentation.
/// </summary>
/// <remarks>
/// Supported formats are <see cref="AsciiDoc"/> and <see cref="Markdown"/>.
/// <see cref="Html"/> is reserved for future use.
/// </remarks>
public enum DocumentationFormat : byte
{
    /// <summary>
    /// Generate documentation in AsciiDoc format (e.g., <c>.adoc</c> files).
    /// </summary>
    AsciiDoc = 0,

    /// <summary>
    /// Generate documentation in GitHub Flavored Markdown (GFM).
    /// </summary>
    Markdown = 1,

    /// <summary>
    /// Reserved for HTML output. Not yet implemented.
    /// </summary>
    Html = 2
}
