using System.Runtime.CompilerServices;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Wasm.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace LionFire.AgUi.Blazor.Wasm.Services;

/// <summary>
/// Connection monitor for Blazor WebAssembly that uses browser APIs to detect connectivity.
/// </summary>
/// <remarks>
/// <para>
/// Uses the Navigator.onLine API and online/offline events to track connectivity state.
/// Implements exponential backoff for reconnection attempts.
/// </para>
/// </remarks>
public sealed class ConnectionMonitor : IConnectionMonitor, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IOptions<WasmAgentClientOptions> _options;
    private readonly ILogger<ConnectionMonitor> _logger;
    private readonly SemaphoreSlim _reconnectLock = new(1, 1);

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ConnectionMonitor>? _dotNetRef;
    private ConnectionState _currentState = ConnectionState.Disconnected;
    private bool _isInitialized;
    private bool _isDisposed;
    private int _failedAttemptCount;
    private DateTimeOffset? _lastConnectedAt;
    private DateTimeOffset? _lastAttemptAt;

    /// <inheritdoc />
    public ConnectionState CurrentState => _currentState;

    /// <inheritdoc />
    public bool IsOnline => _currentState == ConnectionState.Connected;

    /// <inheritdoc />
    public int FailedAttemptCount => _failedAttemptCount;

    /// <inheritdoc />
    public DateTimeOffset? LastConnectedAt => _lastConnectedAt;

    /// <inheritdoc />
    public DateTimeOffset? LastAttemptAt => _lastAttemptAt;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="ConnectionMonitor"/>.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public ConnectionMonitor(
        IJSRuntime jsRuntime,
        IOptions<WasmAgentClientOptions> options,
        ILogger<ConnectionMonitor> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the connection monitor by setting up JavaScript event handlers.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when initialization is done.</returns>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_isInitialized || _isDisposed)
            return;

        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            // Import the connection monitor JavaScript module
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                ct,
                "./_content/LionFire.AgUi.Blazor.Wasm/connectionMonitor.js");

            // Initialize and get current online state
            var isOnline = await _jsModule.InvokeAsync<bool>(
                "initialize",
                ct,
                _dotNetRef);

            UpdateState(isOnline ? ConnectionState.Connected : ConnectionState.Disconnected, "Initial state");
            _isInitialized = true;

            _logger.LogInformation("ConnectionMonitor initialized. Online: {IsOnline}", isOnline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ConnectionMonitor");
            UpdateState(ConnectionState.Error, $"Initialization failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ConnectionState> CheckConnectionAsync(CancellationToken ct = default)
    {
        if (_isDisposed)
            return ConnectionState.Disconnected;

        if (!_isInitialized)
            await InitializeAsync(ct);

        try
        {
            if (_jsModule != null)
            {
                var isOnline = await _jsModule.InvokeAsync<bool>("checkOnline", ct);
                var newState = isOnline ? ConnectionState.Connected : ConnectionState.Disconnected;

                if (newState != _currentState)
                {
                    UpdateState(newState, "Manual check");
                }

                return newState;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking connection status");
            UpdateState(ConnectionState.Error, ex.Message);
        }

        return _currentState;
    }

    /// <inheritdoc />
    public async Task<bool> TryReconnectAsync(CancellationToken ct = default)
    {
        if (_isDisposed)
            return false;

        if (!await _reconnectLock.WaitAsync(0, ct))
        {
            _logger.LogDebug("Reconnect already in progress");
            return false;
        }

        try
        {
            var opts = _options.Value;
            var attempt = 0;
            var delay = opts.ReconnectDelay;

            UpdateState(ConnectionState.Reconnecting, "Manual reconnect initiated");

            while (attempt < opts.MaxReconnectAttempts && !ct.IsCancellationRequested)
            {
                attempt++;
                _lastAttemptAt = DateTimeOffset.UtcNow;

                _logger.LogInformation("Reconnection attempt {Attempt}/{MaxAttempts}", attempt, opts.MaxReconnectAttempts);

                try
                {
                    var state = await CheckConnectionAsync(ct);
                    if (state == ConnectionState.Connected)
                    {
                        _failedAttemptCount = 0;
                        _lastConnectedAt = DateTimeOffset.UtcNow;
                        _logger.LogInformation("Reconnection successful on attempt {Attempt}", attempt);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Reconnection attempt {Attempt} failed", attempt);
                }

                _failedAttemptCount = attempt;

                if (attempt < opts.MaxReconnectAttempts)
                {
                    // Exponential backoff with jitter
                    var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
                    var actualDelay = delay + jitter;

                    if (actualDelay > opts.MaxReconnectDelay)
                        actualDelay = opts.MaxReconnectDelay;

                    _logger.LogDebug("Waiting {Delay} before next attempt", actualDelay);
                    await Task.Delay(actualDelay, ct);

                    // Double the delay for next attempt
                    delay = TimeSpan.FromTicks(delay.Ticks * 2);
                }
            }

            UpdateState(ConnectionState.Error, $"Failed after {attempt} attempts");
            return false;
        }
        finally
        {
            _reconnectLock.Release();
        }
    }

    /// <summary>
    /// Called from JavaScript when the online state changes.
    /// </summary>
    /// <param name="isOnline">Whether the browser reports being online.</param>
    [JSInvokable]
    public void OnOnlineStateChanged(bool isOnline)
    {
        _logger.LogInformation("Browser online state changed: {IsOnline}", isOnline);

        if (isOnline)
        {
            _failedAttemptCount = 0;
            _lastConnectedAt = DateTimeOffset.UtcNow;
            UpdateState(ConnectionState.Connected, "Browser reports online");
        }
        else
        {
            UpdateState(ConnectionState.Disconnected, "Browser reports offline");

            // Start auto-reconnect if enabled
            if (_options.Value.EnableAutoReconnect)
            {
                _ = TryReconnectAsync();
            }
        }
    }

    private void UpdateState(ConnectionState newState, string? reason)
    {
        var previousState = _currentState;
        if (previousState == newState)
            return;

        _currentState = newState;

        _logger.LogDebug(
            "Connection state changed: {PreviousState} -> {NewState} ({Reason})",
            previousState,
            newState,
            reason);

        StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previousState, newState, reason));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _dotNetRef?.Dispose();
        _reconnectLock.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the connection monitor.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose");
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Expected during page unload
            }
        }

        _dotNetRef?.Dispose();
        _reconnectLock.Dispose();
    }
}
