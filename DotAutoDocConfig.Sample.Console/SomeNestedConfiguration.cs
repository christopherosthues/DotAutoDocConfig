namespace DotAutoDocConfig.Sample.Console;

/// <summary>
/// Represents a nested configuration example.
/// </summary>
public class SomeNestedConfiguration
{
    /// <summary>
    /// A sample nested property.
    /// </summary>
    public string NestedProperty { get; set; } = "NestedValue";

    /// <summary>
    /// another level of nested configuration
    /// </summary>
    public SomeOtherNestedConfiguration Nested { get; set; } = new();
}
