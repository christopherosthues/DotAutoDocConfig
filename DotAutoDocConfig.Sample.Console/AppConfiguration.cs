using DotAutoDocConfig.Core.ComponentModel;
using DotAutoDocConfig.Core.ComponentModel.Attributes;

namespace DotAutoDocConfig.Sample.Console;

/// <summary>
/// Represents application-level configuration options used by the sample console app.
/// </summary>
[Documentation(DocumentationFormat.Markdown, "docs/md", ComplexParameterFormat.SeparateTables, includeNamespaces: true)]
[Documentation(DocumentationFormat.AsciiDoc, "docs/asciidoc", includeNamespaces: true)]
[Documentation(DocumentationFormat.Html, "docs/html", includeNamespaces: false)]
[Documentation(DocumentationFormat.Html, "docs/html/tables", ComplexParameterFormat.SeparateTables, includeNamespaces: false)]
public class AppConfiguration
{
    /// <summary>
    /// Maximum number of items the application should process or return.
    /// </summary>
    /// <example>50</example>
    public int MaxItems { get; set; } = 100;

    /// <summary>
    /// Human-friendly name of the application.
    /// </summary>
    public string ApplicationName { get; set; } = "DotAutoDocConfig Sample App";

    /// <summary>
    /// Enables or disables application logging.
    /// </summary>
    /// <example>false</example>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Default operation timeout in seconds for long-running tasks.
    /// </summary>
    public double TimeoutInSeconds { get; set; } = 30.0;

    /// <summary>
    /// List of supported UI or locale language codes (ISO 639-1, e.g., "en").
    /// </summary>
    public string[] SupportedLanguages { get; set; } = ["en", "es", "fr"];

    /// <summary>
    /// Database-related configuration.
    /// </summary>
    public DatabaseConfiguration Database { get; set; } = new();

    /// <summary>
    /// Number of retry attempts for internal operations.
    /// </summary>
    [ExcludeFromDocumentation("This property is for internal use only.")]
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Logging level for application logs.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// An example of nested configuration.
    /// </summary>
    public SomeNestedConfiguration NestedConfig { get; set; } = new();
}
