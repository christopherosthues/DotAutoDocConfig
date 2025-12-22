using DotAutoDocConfig.Core.ComponentModel;
using DotAutoDocConfig.Core.ComponentModel.Attributes;

namespace DotAutoDocConfig.Sample.Console;

/// <summary>
/// Represents another nested configuration example.
/// </summary>
[Documentation(DocumentationFormat.Markdown, "docs/md", ComplexParameterFormat.SeparateTables, includeNamespaces: true)]
public class SomeOtherNestedConfiguration
{
    /// <summary>
    /// Another sample nested property.
    /// </summary>
    public int AnotherNestedProperty { get; set; } = 42;
}
