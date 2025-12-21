namespace LionFire.AgUi.Blazor.Wasm.Configuration;

/// <summary>
/// Configuration options for the WASM agent client factory.
/// </summary>
public sealed class WasmAgentClientOptions
{
    /// <summary>
    /// Gets or sets the base URL for the AG-UI server endpoint.
    /// </summary>
    /// <example>
    /// <code>
    /// options.ServerUrl = "https://api.example.com";
    /// </code>
    /// </example>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// Gets or sets the HTTP timeout for requests.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the timeout for streaming operations.
    /// Default is 5 minutes.
    /// </summary>
    public TimeSpan StreamingTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether to enable automatic reconnection on connection loss.
    /// Default is true.
    /// </summary>
    public bool EnableAutoReconnect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of reconnection attempts.
    /// Default is 5.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the initial delay between reconnection attempts.
    /// Subsequent attempts use exponential backoff.
    /// Default is 1 second.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum delay between reconnection attempts.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to queue messages when offline.
    /// Default is true.
    /// </summary>
    public bool EnableOfflineQueue { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of messages to queue when offline.
    /// Default is 100.
    /// </summary>
    public int MaxQueuedMessages { get; set; } = 100;
}
