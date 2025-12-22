namespace DotAutoDocConfig.Sample.Console;

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
