using DotAutoDocConfig.Core.ComponentModel;
using DotAutoDocConfig.Core.ComponentModel.Attributes;

namespace DotAutoDocConfig.Sample.Console;

/// <summary>
///
/// </summary>
[Documentation(DocumentationFormat.Markdown, "docs/AppConfiguration.md")]
[Documentation(DocumentationFormat.AsciiDoc, "docs/AppConfiguration.adoc")]
public class AppConfiguration
{
    public int MaxItems { get; set; } = 100;

    public string ApplicationName { get; set; } = "DotAutoDocConfig Sample App";

    public bool EnableLogging { get; set; } = true;

    public double TimeoutInSeconds { get; set; } = 30.0;

    public string[] SupportedLanguages { get; set; } = ["en", "es", "fr"];

    public DatabaseConfiguration Database { get; set; } = new();

    [ExcludeFromDocumentation("This property is for internal use only.")]
    public int RetryAttempts { get; set; } = 3;
}

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=mydb;User Id=myuser;Password=mypassword;";

    public int MaxConnections { get; set; } = 10;

    [ExcludeFromDocumentation("This property is for internal use only.")]
    public bool UseSsl { get; set; } = false;

    public int CommandTimeout { get; set; } = 60;
}
