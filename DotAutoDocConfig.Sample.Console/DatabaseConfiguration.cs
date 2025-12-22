using DotAutoDocConfig.Core.ComponentModel.Attributes;

namespace DotAutoDocConfig.Sample.Console;

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
