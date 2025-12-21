using System.Text.Json;
using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A dialog component for approving or denying a tool call with detailed information.
/// </summary>
/// <remarks>
/// <para>
/// This dialog displays comprehensive information about a tool call including:
/// <list type="bullet">
/// <item><description>Tool name and description</description></item>
/// <item><description>Risk level with visual indicator</description></item>
/// <item><description>Formatted JSON arguments</description></item>
/// <item><description>Request timestamp</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var parameters = new DialogParameters&lt;MudToolApprovalDialog&gt;
/// {
///     { x =&gt; x.ToolCall, toolCall }
/// };
/// var dialog = await DialogService.ShowAsync&lt;MudToolApprovalDialog&gt;("Approve Tool", parameters);
/// </code>
/// </example>
// Note: Static properties are used to work around Razor generator issues with MudBlazor enums
// when @using MudBlazor is declared in the .razor file

public partial class MudToolApprovalDialog : ComponentBase
{
    // Static MudBlazor enum helpers for Razor template
    protected static Size SizeSmall => Size.Small;
    protected static Typo TypoH6 => Typo.h6;
    protected static Typo TypoBody1 => Typo.body1;
    protected static Typo TypoBody2 => Typo.body2;
    protected static Typo TypoSubtitle2 => Typo.subtitle2;
    protected static Color ColorSuccess => Color.Success;
    protected static Color ColorError => Color.Error;
    protected static Variant VariantText => Variant.Text;
    protected static Variant VariantOutlined => Variant.Outlined;
    protected static Variant VariantFilled => Variant.Filled;
    protected static Severity SeverityWarning => Severity.Warning;
    protected static string IconCheck => Icons.Material.Filled.Check;
    protected static string IconClose => Icons.Material.Filled.Close;

    /// <summary>
    /// Gets or sets the tool call to display for approval.
    /// </summary>
    [Parameter]
    public ToolCall ToolCall { get; set; } = default!;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    /// <summary>
    /// Approves the tool call and closes the dialog.
    /// </summary>
    protected void Approve()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }

    /// <summary>
    /// Denies the tool call and closes the dialog.
    /// </summary>
    protected void Deny()
    {
        MudDialog.Close(DialogResult.Ok(false));
    }

    /// <summary>
    /// Cancels the dialog without making a decision.
    /// </summary>
    protected void Cancel()
    {
        MudDialog.Cancel();
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
}
