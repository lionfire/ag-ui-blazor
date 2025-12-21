using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Monitors network connectivity and provides connection state information.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts network connectivity monitoring, allowing different implementations
/// for various environments (browser, server, mobile).
/// </para>
/// <para>
/// <strong>Blazor WASM Implementation:</strong>
/// Uses the browser's Navigator.onLine API and online/offline events to track connectivity.
/// </para>
/// <para>
/// <strong>Blazor Server Implementation:</strong>
/// May use SignalR circuit state or periodic connectivity checks.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// @inject IConnectionMonitor ConnectionMonitor
///
/// protected override void OnInitialized()
/// {
///     ConnectionMonitor.StateChanged += OnStateChanged;
/// }
///
/// private void OnStateChanged(object? sender, ConnectionStateChangedEventArgs e)
/// {
///     // Handle state change
///     StateHasChanged();
/// }
/// </code>
/// </example>
public interface IConnectionMonitor : IDisposable
{
    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    ConnectionState CurrentState { get; }

    /// <summary>
    /// Gets whether the client is currently online.
    /// </summary>
    bool IsOnline { get; }

    /// <summary>
    /// Occurs when the connection state changes.
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Manually checks the connection status and updates the state.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes with the current connection state.</returns>
    Task<ConnectionState> CheckConnectionAsync(CancellationToken ct = default);

    /// <summary>
    /// Initiates a manual reconnection attempt.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes with true if reconnection succeeded; otherwise, false.</returns>
    Task<bool> TryReconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the number of consecutive failed connection attempts.
    /// </summary>
    int FailedAttemptCount { get; }

    /// <summary>
    /// Gets the time of the last successful connection.
    /// </summary>
    DateTimeOffset? LastConnectedAt { get; }

    /// <summary>
    /// Gets the time of the last connection attempt.
    /// </summary>
    DateTimeOffset? LastAttemptAt { get; }
}

/// <summary>
/// Event arguments for connection state changes.
/// </summary>
public sealed class ConnectionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="ConnectionStateChangedEventArgs"/>.
    /// </summary>
    /// <param name="previousState">The previous connection state.</param>
    /// <param name="newState">The new connection state.</param>
    /// <param name="reason">An optional reason for the state change.</param>
    public ConnectionStateChangedEventArgs(
        ConnectionState previousState,
        ConnectionState newState,
        string? reason = null)
    {
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the previous connection state.
    /// </summary>
    public ConnectionState PreviousState { get; }

    /// <summary>
    /// Gets the new connection state.
    /// </summary>
    public ConnectionState NewState { get; }

    /// <summary>
    /// Gets an optional reason for the state change.
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Gets the timestamp when the state change occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}
