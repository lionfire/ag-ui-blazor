using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A simple component that displays the current connection state with a colored indicator dot.
/// </summary>
/// <remarks>
/// <para>
/// Connection states are displayed as follows:
/// <list type="bullet">
/// <item><description>Connected: Green dot</description></item>
/// <item><description>Connecting/Reconnecting: Yellow pulsing dot</description></item>
/// <item><description>Disconnected/Error: Red dot</description></item>
/// </list>
/// </para>
/// <para>
/// When a <see cref="ConnectionMonitor"/> is provided, the component will automatically
/// update when the connection state changes and show a retry button when disconnected.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;MudConnectionStatus State="@ConnectionState.Connected" ShowLabel="true" /&gt;
/// </code>
/// </example>
public partial class MudConnectionStatus : ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the current connection state to display.
    /// </summary>
    [Parameter]
    public ConnectionState State { get; set; } = ConnectionState.Disconnected;

    /// <summary>
    /// Gets or sets whether to show the text label alongside the status dot.
    /// Default is false (only shows the dot).
    /// </summary>
    [Parameter]
    public bool ShowLabel { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to show a retry button when disconnected.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool ShowRetryButton { get; set; } = false;

    /// <summary>
    /// Gets or sets the connection monitor to use for automatic state updates and retry.
    /// </summary>
    [Parameter]
    public IConnectionMonitor? ConnectionMonitor { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _isRetrying;
    private bool _disposed;

    /// <summary>
    /// Gets the CSS class for the status container based on the current state.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    protected string GetStatusClass()
    {
        var stateClass = State switch
        {
            ConnectionState.Connected => "status-connected",
            ConnectionState.Connecting => "status-connecting",
            ConnectionState.Reconnecting => "status-reconnecting",
            ConnectionState.Disconnected => "status-disconnected",
            ConnectionState.Error => "status-error",
            _ => "status-unknown"
        };

        return string.IsNullOrWhiteSpace(Class) ? stateClass : $"{stateClass} {Class}";
    }

    /// <summary>
    /// Gets the CSS class for the status dot based on the current state.
    /// </summary>
    /// <returns>The CSS class string for the dot.</returns>
    protected string GetDotClass()
    {
        return State switch
        {
            ConnectionState.Connected => "dot-green",
            ConnectionState.Connecting => "dot-yellow dot-pulse",
            ConnectionState.Reconnecting => "dot-yellow dot-pulse",
            ConnectionState.Disconnected => "dot-red",
            ConnectionState.Error => "dot-red",
            _ => "dot-gray"
        };
    }

    /// <summary>
    /// Gets the title/tooltip text for the status indicator.
    /// </summary>
    /// <returns>The title text.</returns>
    protected string GetStatusTitle()
    {
        return State switch
        {
            ConnectionState.Connected => "Connected",
            ConnectionState.Connecting => "Connecting...",
            ConnectionState.Reconnecting => "Reconnecting...",
            ConnectionState.Disconnected => "Disconnected",
            ConnectionState.Error => "Connection Error",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the text label for the status indicator.
    /// </summary>
    /// <returns>The label text.</returns>
    protected string GetStatusLabel()
    {
        return GetStatusTitle();
    }

    /// <summary>
    /// Gets the effective connection state, using the monitor if available.
    /// </summary>
    protected ConnectionState GetEffectiveState()
    {
        return ConnectionMonitor?.CurrentState ?? State;
    }

    /// <summary>
    /// Gets whether the retry button should be shown.
    /// </summary>
    protected bool ShouldShowRetryButton()
    {
        if (!ShowRetryButton || ConnectionMonitor == null)
            return false;

        var state = GetEffectiveState();
        return state == ConnectionState.Disconnected || state == ConnectionState.Error;
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Subscribe to connection monitor state changes if we haven't already
        if (ConnectionMonitor != null && !_disposed)
        {
            ConnectionMonitor.StateChanged -= OnConnectionStateChanged;
            ConnectionMonitor.StateChanged += OnConnectionStateChanged;
        }
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handles the retry button click.
    /// </summary>
    protected async Task OnRetryClickAsync()
    {
        if (ConnectionMonitor == null || _isRetrying)
            return;

        _isRetrying = true;
        StateHasChanged();

        try
        {
            await ConnectionMonitor.TryReconnectAsync();
        }
        finally
        {
            _isRetrying = false;
            StateHasChanged();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (ConnectionMonitor != null)
        {
            ConnectionMonitor.StateChanged -= OnConnectionStateChanged;
        }
    }
}
