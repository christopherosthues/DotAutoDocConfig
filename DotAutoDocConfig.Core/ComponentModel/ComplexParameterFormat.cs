namespace DotAutoDocConfig.Core.ComponentModel;

/// <summary>
/// Defines how complex (object-typed) configuration parameters are represented
/// in generated documentation.
/// </summary>
/// <remarks>
/// This setting is intended for nested objects discovered by the documentation
/// source generator. It controls whether child members are shown inline or in
/// dedicated sections/tables for improved readability.
/// </remarks>
public enum ComplexParameterFormat
{
    /// <summary>
    /// Render complex properties inline as a short JSON object example on the
    /// parent row.
    /// </summary>
    /// <example>Child:Child:Child</example>
    /// <remarks>
    /// Best for small objects (roughly 1–3 fields). Keeps a single table compact
    /// while still conveying structure.
    /// </remarks>
    InlineJsonShort,

    /// <summary>
    /// Render complex properties in separate sections/tables with their own rows.
    /// </summary>
    /// <remarks>
    /// Recommended for larger or deeply nested objects. Improves readability at
    /// the cost of longer documents.
    /// </remarks>
    SeparateTables
}
