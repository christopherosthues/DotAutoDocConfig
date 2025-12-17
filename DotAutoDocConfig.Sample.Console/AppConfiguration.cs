using DotAutoDocConfig.Core.ComponentModel;
using DotAutoDocConfig.Core.ComponentModel.Attributes;

namespace DotAutoDocConfig.Sample.Console;

/// <summary>
/// Represents application-level configuration options used by the sample console app.
/// </summary>
[Documentation(DocumentationFormat.Markdown, "docs/AppConfiguration.md", ComplexParameterFormat.SeparateTables)]
[Documentation(DocumentationFormat.AsciiDoc, "docs/", includeNamespaces: true)]
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
}

/// <summary>
/// Contains database connection and command execution settings.
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// ADO.NET-compatible connection string used to connect to the database.
    /// </summary>
    public string ConnectionString { get; set; } = "Server=localhost;Database=mydb;User Id=myuser;Password=mypassword;";

    /// <summary>
    /// Maximum number of concurrent connections allowed by the application.
    /// </summary>
    /// <example>5</example>
    public int MaxConnections { get; set; } = 10;

    /// <summary>
    /// Indicates whether to require SSL/TLS for database connections.
    /// </summary>
    [ExcludeFromDocumentation("This property is for internal use only.")]
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Timeout in seconds applied to database commands.
    /// </summary>
    public int CommandTimeout { get; set; } = 60;
}

/// <summary>
/// The logging level for application logs.
/// </summary>
public enum LogLevel
{
    /// <summary>No logging output.</summary>
    None,
    /// <summary>Very detailed diagnostic information, potentially high volume.</summary>
    Trace,
    /// <summary>Fine-grained events useful for debugging.</summary>
    Debug,
    /// <summary>Informational messages that highlight the progress of the application.</summary>
    Information,
    /// <summary>Potentially harmful situations that are not necessarily errors.</summary>
    Warning,
    /// <summary>Errors that prevent normal execution of a specific operation.</summary>
    Error,
    /// <summary>Critical errors causing premature termination or severe failures.</summary>
    Critical,
}
