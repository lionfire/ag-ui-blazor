using LionFire.AgUi.Blazor.ErrorHandling;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

// Note: Static properties are used to work around Razor generator issues with MudBlazor enums
// when @using MudBlazor is declared in the .razor file

/// <summary>
/// A component that displays agent errors using MudBlazor alerts with retry support.
/// </summary>
/// <remarks>
/// <para>
/// This component provides user-friendly error display with:
/// <list type="bullet">
/// <item><description>Severity-based color coding</description></item>
/// <item><description>Actionable guidance for users</description></item>
/// <item><description>Retry button for recoverable errors</description></item>
/// <item><description>Optional technical details expansion</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;MudAgentError Error="@currentError"
///                OnRetry="HandleRetryClicked"
///                ShowGuidance="true" /&gt;
/// </code>
/// </example>
public partial class MudAgentError : ComponentBase
{
    // Static MudBlazor enum helpers for Razor template
    protected static Variant AlertVariant => Variant.Filled;
    protected static Variant RetryButtonVariant => Variant.Text;
    protected static Size IconSize => Size.Small;
    protected static Size ButtonSizeSmall => Size.Small;
    protected static Size ProgressSizeSmall => Size.Small;
    protected static Typo MessageTypo => Typo.body2;
    protected static Typo GuidanceTypo => Typo.caption;
    protected static Color ColorPrimary => Color.Primary;
    protected static string IconRefresh => Icons.Material.Filled.Refresh;

    /// <summary>
    /// Gets or sets the error to display.
    /// </summary>
    [Parameter]
    public AgentError? Error { get; set; }

    /// <summary>
    /// Gets or sets whether the component is compact.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool Dense { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to show the retry button for retryable errors.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowRetryButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show actionable guidance.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowGuidance { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show technical details.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool ShowTechnicalDetails { get; set; } = false;

    /// <summary>
    /// Gets or sets the callback invoked when the retry button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnRetry { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the alert.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether a retry is currently in progress.
    /// </summary>
    [Parameter]
    public bool IsRetrying { get; set; }

    /// <summary>
    /// Gets the severity based on the error category.
    /// </summary>
    protected Severity GetSeverity()
    {
        if (Error == null)
            return Severity.Info;

        return Error.Category switch
        {
            AgentErrorCategory.Authentication => Severity.Error,
            AgentErrorCategory.BadRequest => Severity.Error,
            AgentErrorCategory.ContentPolicy => Severity.Warning,
            AgentErrorCategory.ContextLengthExceeded => Severity.Warning,
            AgentErrorCategory.ModelNotFound => Severity.Error,
            AgentErrorCategory.Network => Severity.Warning,
            AgentErrorCategory.RateLimit => Severity.Warning,
            AgentErrorCategory.ServerError => Severity.Error,
            AgentErrorCategory.ServiceUnavailable => Severity.Warning,
            AgentErrorCategory.Timeout => Severity.Warning,
            AgentErrorCategory.Cancelled => Severity.Info,
            AgentErrorCategory.Serialization => Severity.Error,
            _ => Severity.Error
        };
    }

    /// <summary>
    /// Gets the icon based on the error category.
    /// </summary>
    protected string GetIcon()
    {
        if (Error == null)
            return Icons.Material.Filled.Info;

        return Error.Category switch
        {
            AgentErrorCategory.Authentication => Icons.Material.Filled.Lock,
            AgentErrorCategory.BadRequest => Icons.Material.Filled.Error,
            AgentErrorCategory.ContentPolicy => Icons.Material.Filled.Policy,
            AgentErrorCategory.ContextLengthExceeded => Icons.Material.Filled.TextSnippet,
            AgentErrorCategory.ModelNotFound => Icons.Material.Filled.SearchOff,
            AgentErrorCategory.Network => Icons.Material.Filled.WifiOff,
            AgentErrorCategory.RateLimit => Icons.Material.Filled.Speed,
            AgentErrorCategory.ServerError => Icons.Material.Filled.Cloud,
            AgentErrorCategory.ServiceUnavailable => Icons.Material.Filled.CloudOff,
            AgentErrorCategory.Timeout => Icons.Material.Filled.Timer,
            AgentErrorCategory.Cancelled => Icons.Material.Filled.Cancel,
            AgentErrorCategory.Serialization => Icons.Material.Filled.Code,
            _ => Icons.Material.Filled.ErrorOutline
        };
    }

    /// <summary>
    /// Gets the CSS class for the alert.
    /// </summary>
    protected string GetAlertClass()
    {
        var classes = "agent-error-alert";
        if (!string.IsNullOrEmpty(Class))
        {
            classes += $" {Class}";
        }
        return classes;
    }

    /// <summary>
    /// Formats the retry-after duration for display.
    /// </summary>
    protected static string FormatRetryAfter(TimeSpan duration)
    {
        if (duration.TotalSeconds < 60)
        {
            return $"{duration.TotalSeconds:F0} seconds";
        }
        else if (duration.TotalMinutes < 60)
        {
            return $"{duration.TotalMinutes:F0} minutes";
        }
        else
        {
            return $"{duration.TotalHours:F1} hours";
        }
    }

    /// <summary>
    /// Handles the retry button click.
    /// </summary>
    protected async Task HandleRetry()
    {
        await OnRetry.InvokeAsync();
    }
}
