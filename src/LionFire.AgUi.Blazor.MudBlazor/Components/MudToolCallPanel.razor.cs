using System.Text.Json;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

// Note: Static properties are used to work around Razor generator issues with MudBlazor enums
// when @using MudBlazor is declared in the .razor file

/// <summary>
/// A panel component that displays pending tool calls and allows users to approve or deny them.
/// </summary>
/// <remarks>
/// <para>
/// This component integrates with <see cref="ToolApprovalService"/> to display pending approvals
/// and provides buttons for users to approve or deny each tool call.
/// </para>
/// <para>
/// Tool calls are color-coded by risk level:
/// <list type="bullet">
/// <item><description>Safe: Green</description></item>
/// <item><description>Risky: Orange/Warning</description></item>
/// <item><description>Dangerous: Red/Error</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;MudToolCallPanel ToolApprovalService="@approvalService"
///                   ShowArguments="true"
///                   ShowDetailsButton="true" /&gt;
/// </code>
/// </example>
public partial class MudToolCallPanel : ComponentBase, IDisposable
{
    // Static MudBlazor enum helpers for Razor template
    protected static Size SizeSmall => Size.Small;
    protected static Typo TypoBody2 => Typo.body2;
    protected static Typo TypoSubtitle2 => Typo.subtitle2;
    protected static Typo TypoCaption => Typo.caption;
    protected static string IconInfo => Icons.Material.Filled.Info;
    protected static string IconCheck => Icons.Material.Filled.Check;
    protected static string IconClose => Icons.Material.Filled.Close;
    protected static Color ColorSuccess => Color.Success;
    protected static Color ColorError => Color.Error;

    /// <summary>
    /// Gets or sets the tool approval service to monitor for pending approvals.
    /// </summary>
    [Parameter]
    public ToolApprovalService? ToolApprovalService { get; set; }

    /// <summary>
    /// Gets or sets the list of pending tool calls to display.
    /// If not provided, the component will use <see cref="ToolApprovalService"/> to get pending calls.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ToolCall> PendingToolCalls { get; set; } = Array.Empty<ToolCall>();

    /// <summary>
    /// Gets or sets whether to show tool arguments inline.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool ShowArguments { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to show a details button for each tool call.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowDetailsButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback invoked when a tool call is approved.
    /// </summary>
    [Parameter]
    public EventCallback<ToolCall> OnApproved { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a tool call is denied.
    /// </summary>
    [Parameter]
    public EventCallback<ToolCall> OnDenied { get; set; }

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

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private bool _disposed;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (ToolApprovalService != null)
        {
            ToolApprovalService.ApprovalRequested += OnApprovalRequested;
            ToolApprovalService.ApprovalResolved += OnApprovalResolved;
            RefreshPendingCalls();
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ToolApprovalService != null && PendingToolCalls.Count == 0)
        {
            RefreshPendingCalls();
        }
    }

    private void RefreshPendingCalls()
    {
        if (ToolApprovalService != null)
        {
            PendingToolCalls = ToolApprovalService.GetPendingApprovals();
        }
    }

    private void OnApprovalRequested(object? sender, ToolCall toolCall)
    {
        InvokeAsync(() =>
        {
            RefreshPendingCalls();
            StateHasChanged();
        });
    }

    private void OnApprovalResolved(object? sender, ToolApprovalAuditEntry entry)
    {
        InvokeAsync(() =>
        {
            RefreshPendingCalls();
            StateHasChanged();
        });
    }

    /// <summary>
    /// Handles approval of a tool call.
    /// </summary>
    protected async Task OnApprove(ToolCall toolCall)
    {
        ToolApprovalService?.Approve(toolCall.Id);
        await OnApproved.InvokeAsync(toolCall);
    }

    /// <summary>
    /// Handles denial of a tool call.
    /// </summary>
    protected async Task OnDeny(ToolCall toolCall)
    {
        ToolApprovalService?.Deny(toolCall.Id, "User denied");
        await OnDenied.InvokeAsync(toolCall);
    }

    /// <summary>
    /// Shows a dialog with detailed information about the tool call.
    /// </summary>
    protected async Task ShowDetailsDialog(ToolCall toolCall)
    {
        var parameters = new DialogParameters<MudToolApprovalDialog>
        {
            { x => x.ToolCall, toolCall }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<MudToolApprovalDialog>(
            $"Tool Call: {toolCall.Name}",
            parameters,
            options);

        var result = await dialog.Result;
        if (result != null && !result.Canceled)
        {
            if (result.Data is bool approved && approved)
            {
                await OnApprove(toolCall);
            }
            else
            {
                await OnDeny(toolCall);
            }
        }
    }

    /// <summary>
    /// Gets the CSS class for a tool call based on its risk level.
    /// </summary>
    protected static string GetToolCallClass(ToolCall toolCall)
    {
        return toolCall.RiskLevel switch
        {
            ToolRiskLevel.Safe => "tool-call-safe",
            ToolRiskLevel.Risky => "tool-call-risky",
            ToolRiskLevel.Dangerous => "tool-call-dangerous",
            _ => "tool-call-unknown"
        };
    }

    /// <summary>
    /// Gets the icon for a risk level.
    /// </summary>
    protected static string GetRiskIcon(ToolRiskLevel riskLevel)
    {
        return riskLevel switch
        {
            ToolRiskLevel.Safe => Icons.Material.Filled.CheckCircle,
            ToolRiskLevel.Risky => Icons.Material.Filled.Warning,
            ToolRiskLevel.Dangerous => Icons.Material.Filled.Dangerous,
            _ => Icons.Material.Filled.Help
        };
    }

    /// <summary>
    /// Gets the color for a risk level.
    /// </summary>
    protected static Color GetRiskColor(ToolRiskLevel riskLevel)
    {
        return riskLevel switch
        {
            ToolRiskLevel.Safe => Color.Success,
            ToolRiskLevel.Risky => Color.Warning,
            ToolRiskLevel.Dangerous => Color.Error,
            _ => Color.Default
        };
    }

    /// <summary>
    /// Formats tool arguments as formatted JSON.
    /// </summary>
    protected static string FormatArguments(IReadOnlyDictionary<string, object>? arguments)
    {
        if (arguments == null || arguments.Count == 0)
            return "{}";

        try
        {
            return JsonSerializer.Serialize(arguments, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return "{ /* Error formatting arguments */ }";
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (ToolApprovalService != null)
        {
            ToolApprovalService.ApprovalRequested -= OnApprovalRequested;
            ToolApprovalService.ApprovalResolved -= OnApprovalResolved;
        }
    }
}
