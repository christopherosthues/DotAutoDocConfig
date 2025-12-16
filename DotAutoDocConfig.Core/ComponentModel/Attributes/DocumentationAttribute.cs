using System;

namespace DotAutoDocConfig.Core.ComponentModel.Attributes;

/// <summary>
/// Instructs the documentation source generator to produce documentation for the
/// annotated configuration class.
/// </summary>
/// <remarks>
/// - The attribute can be applied multiple times to the same type to generate
///   multiple outputs (e.g., Markdown and AsciiDoc, or different destinations).
/// - <paramref name="outputPath"/> may be relative or absolute. Relative paths are
///   resolved against the project root (the directory of the .csproj). Absolute
///   paths are used as-is.
/// - <paramref name="complexParameterFormat"/> controls how complex (class/record)
///   properties are represented:
///   <list type="bullet">
///     <item>
///       <description>
///         <see cref="ComplexParameterFormat.InlineJsonShort"/> flattens nested
///         properties into a single table using JsonShort notation with colon (:) as
///         separator. Example key path: <c>Property:Child:GrandChild</c>.
///       </description>
///     </item>
///     <item>
///       <description>
///         <see cref="ComplexParameterFormat.SeparateTables"/> generates separate
///         tables (and, depending on the renderer, separate files) for nested
///         configuration types. The root table links to the nested sections/files.
///       </description>
///     </item>
///   </list>
/// - <paramref name="includeNamespaces"/> can be used to include namespaces in
///   headings or labels to disambiguate types that share the same name across
///   different namespaces.
/// </remarks>
/// <param name="format">Output format for the generated documentation. Supported values include
/// <see cref="DocumentationFormat.Markdown"/> and <see cref="DocumentationFormat.AsciiDoc"/>.
/// HTML is reserved.</param>
/// <param name="outputPath">Target file path for the generated documentation. Relative paths are
/// resolved against the project root; absolute paths are written directly.</param>
/// <param name="complexParameterFormat">Controls the representation of complex nested types:
/// <see cref="ComplexParameterFormat.InlineJsonShort"/> (uses JsonShort notation with
/// <c>:</c> separators) or <see cref="ComplexParameterFormat.SeparateTables"/> (nested tables/files).
/// Defaults to <see cref="ComplexParameterFormat.InlineJsonShort"/>.</param>
/// <param name="includeNamespaces">If <c>true</c>, namespaces may be included in headings and/or link labels
/// to help distinguish types that share the same name across different namespaces especially when using
/// <see cref="ComplexParameterFormat.SeparateTables"/> with the same output path.
/// Defaults to <c>false</c>.</param>
/// <example>
/// <code>
/// using DotAutoDocConfig.Core.ComponentModel;
/// using DotAutoDocConfig.Core.ComponentModel.Attributes;
///
/// [Documentation(DocumentationFormat.Markdown, "docs/AppConfiguration.md")]
/// public class AppConfiguration { }
///
/// // Generate AsciiDoc with separate tables and include namespaces in headings
/// [Documentation(
///     DocumentationFormat.AsciiDoc,
///     "docs/AppConfiguration.adoc",
///     ComplexParameterFormat.SeparateTables,
///     includeNamespaces: true)]
/// public class AppConfiguration2 { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class DocumentationAttribute(
    DocumentationFormat format,
    string outputPath,
    ComplexParameterFormat complexParameterFormat = ComplexParameterFormat.InlineJsonShort,
    bool includeNamespaces = false) : Attribute
{
    /// <summary>
    /// Selected output format of the generated documentation (e.g., Markdown or AsciiDoc).
    /// </summary>
    public DocumentationFormat Format { get; } = format;

    /// <summary>
    /// Controls how complex (object-typed) properties are represented in the
    /// generated documentation (inline via JsonShort or as separate tables).
    /// </summary>
    public ComplexParameterFormat ComplexParameterFormat { get; } = complexParameterFormat;

    /// <summary>
    /// When enabled, namespaces may be included in headings and labels to
    /// disambiguate types with identical names.
    /// </summary>
    public bool IncludeNamespaces { get; } = includeNamespaces;

    /// <summary>
    /// Destination path for the generated output. Relative paths are resolved
    /// against the project root; absolute paths are written as-is.
    /// </summary>
    public string OutputPath { get; set; } = outputPath;
}
