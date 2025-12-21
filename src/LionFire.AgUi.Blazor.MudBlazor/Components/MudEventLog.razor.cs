using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A component that displays a log of events for debugging and monitoring.
/// </summary>
public partial class MudEventLog : ComponentBase
{
    #region MudBlazor Constants

    protected static string IconDelete => Icons.Material.Filled.Delete;
    protected static string IconInfo => Icons.Material.Filled.Info;
    protected static string IconWarning => Icons.Material.Filled.Warning;
    protected static string IconError => Icons.Material.Filled.Error;
    protected static string IconDebug => Icons.Material.Filled.BugReport;
    protected static Size SmallSize => Size.Small;
    protected static Color TertiaryColor => Color.Tertiary;
    protected static Color InfoColor => Color.Info;
    protected static Color WarningColor => Color.Warning;
    protected static Color ErrorColor => Color.Error;
    protected static Color DefaultColor => Color.Default;
    protected static Typo TypoSubtitle2 => Typo.subtitle2;
    protected static Typo TypoBody2 => Typo.body2;
    protected static Typo TypoCaption => Typo.caption;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the title of the event log.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Event Log";

    /// <summary>
    /// Gets or sets the collection of events to display.
    /// </summary>
    [Parameter]
    public IReadOnlyList<EventLogEntry> Events { get; set; } = Array.Empty<EventLogEntry>();

    /// <summary>
    /// Gets or sets the maximum number of visible events.
    /// </summary>
    [Parameter]
    public int MaxVisibleEvents { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether to show the clear button.
    /// </summary>
    [Parameter]
    public bool ShowClearButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the message to display when no events are present.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No events recorded";

    /// <summary>
    /// Gets or sets the callback invoked when the clear button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnClear { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    #endregion

    #region Methods

    private IEnumerable<EventLogEntry> GetVisibleEvents()
    {
        return Events.TakeLast(MaxVisibleEvents).Reverse();
    }

    private static string GetEventClass(EventLogEntry entry)
    {
        return entry.Level switch
        {
            EventLogLevel.Error => "event-error",
            EventLogLevel.Warning => "event-warning",
            EventLogLevel.Info => "event-info",
            EventLogLevel.Debug => "event-debug",
            _ => ""
        };
    }

    private static string GetEventIcon(EventLogLevel level)
    {
        return level switch
        {
            EventLogLevel.Error => Icons.Material.Filled.Error,
            EventLogLevel.Warning => Icons.Material.Filled.Warning,
            EventLogLevel.Info => Icons.Material.Filled.Info,
            EventLogLevel.Debug => Icons.Material.Filled.BugReport,
            _ => Icons.Material.Filled.Info
        };
    }

    private static Color GetEventColor(EventLogLevel level)
    {
        return level switch
        {
            EventLogLevel.Error => Color.Error,
            EventLogLevel.Warning => Color.Warning,
            EventLogLevel.Info => Color.Info,
            EventLogLevel.Debug => Color.Default,
            _ => Color.Default
        };
    }

    private async Task HandleClearAsync()
    {
        if (OnClear.HasDelegate)
        {
            await OnClear.InvokeAsync();
        }
    }

    #endregion
}

/// <summary>
/// Represents an entry in the event log.
/// </summary>
/// <param name="Timestamp">The timestamp when the event occurred.</param>
/// <param name="EventType">The type of event.</param>
/// <param name="Message">The event message.</param>
/// <param name="Level">The severity level of the event.</param>
/// <param name="Details">Optional additional details.</param>
public sealed record EventLogEntry(
    DateTimeOffset Timestamp,
    string EventType,
    string Message,
    EventLogLevel Level = EventLogLevel.Info,
    string? Details = null
);

/// <summary>
/// Represents the severity level of an event log entry.
/// </summary>
public enum EventLogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
