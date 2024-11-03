namespace SharpMeta;

/// <summary>
/// Represents a logger for load context events.
/// </summary>
public class SharpResolverLogger
{
    /// <summary>
    /// Gets the default logger instance.
    /// </summary>
    public static readonly SharpResolverLogger Default = new();

    /// <summary>
    /// Gets the console logger instance which logs to the system console.
    /// </summary>
    public static readonly SharpResolverLogger Console = new()
    {
        OnInfo = System.Console.WriteLine,
        OnWarning = System.Console.WriteLine,
        OnError = System.Console.WriteLine
    };

    /// <summary>
    /// Gets or sets the action to be invoked for informational messages.
    /// </summary>
    public Action<string>? OnInfo { get; init; } = null;

    /// <summary>
    /// Gets or sets the action to be invoked for warning messages.
    /// </summary>
    public Action<string>? OnWarning { get; init; } = null;

    /// <summary>
    /// Gets or sets the action to be invoked for error messages.
    /// </summary>
    public Action<string>? OnError { get; init; } = null;
}
