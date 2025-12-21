using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// Display variants for the token summary.
/// </summary>
public enum TokenSummaryVariant
{
    /// <summary>
    /// A compact chip with tooltip for details.
    /// </summary>
    Compact,

    /// <summary>
    /// An inline text display.
    /// </summary>
    Inline,

    /// <summary>
    /// A detailed card with all information visible.
    /// </summary>
    Detailed
}

// Note: Static properties are used to work around Razor generator issues with MudBlazor enums
// when @using MudBlazor is declared in the .razor file

/// <summary>
/// A component that displays token usage statistics and estimated costs.
/// </summary>
/// <remarks>
/// <para>
/// This component supports multiple display variants:
/// <list type="bullet">
/// <item><description>Compact: A small chip with hover tooltip</description></item>
/// <item><description>Inline: Simple text display</description></item>
/// <item><description>Detailed: Full card with all statistics</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;MudTokenSummary TokenUsage="@viewModel.TokenUsage"
///                  Variant="TokenSummaryVariant.Detailed"
///                  ShowCost="true" /&gt;
/// </code>
/// </example>
public partial class MudTokenSummary : ComponentBase
{
    // Static MudBlazor enum helpers for Razor template
    protected static Size ChipSizeSmall => Size.Small;
    protected static Size IconSizeSmall => Size.Small;
    protected static Color ColorDefault => Color.Default;
    protected static Color ColorInfo => Color.Info;
    protected static Color ColorSuccess => Color.Success;
    protected static global::MudBlazor.Variant ChipVariantText => global::MudBlazor.Variant.Text;
    protected static global::MudBlazor.Variant ChipVariantOutlined => global::MudBlazor.Variant.Outlined;
    protected static Typo TypoSubtitle2 => Typo.subtitle2;
    protected static Typo TypoCaption => Typo.caption;
    protected static Typo TypoBody2 => Typo.body2;
    protected static string IconToken => Icons.Material.Filled.Token;

    /// <summary>
    /// Gets or sets the token usage to display.
    /// </summary>
    [Parameter]
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Gets or sets the display variant.
    /// Default is <see cref="TokenSummaryVariant.Compact"/>.
    /// </summary>
    [Parameter]
    public TokenSummaryVariant Variant { get; set; } = TokenSummaryVariant.Compact;

    /// <summary>
    /// Gets or sets whether to show cost estimates.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowCost { get; set; } = true;

    /// <summary>
    /// Gets or sets the currency symbol for cost display.
    /// Default is "$".
    /// </summary>
    [Parameter]
    public string CurrencySymbol { get; set; } = "$";

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the container CSS class.
    /// </summary>
    protected string GetContainerClass()
    {
        var classes = "token-summary";
        if (!string.IsNullOrEmpty(Class))
        {
            classes += $" {Class}";
        }
        return classes;
    }

    /// <summary>
    /// Gets the tooltip text for compact variant.
    /// </summary>
    protected string GetTooltipText()
    {
        if (TokenUsage == null)
            return string.Empty;

        var text = $"Prompt: {FormatTokenCount(TokenUsage.PromptTokens)}\n" +
                   $"Completion: {FormatTokenCount(TokenUsage.CompletionTokens)}\n" +
                   $"Total: {FormatTokenCount(TokenUsage.TotalTokens)}";

        if (ShowCost && TokenUsage.EstimatedCost.HasValue)
        {
            text += $"\nEstimated Cost: {FormatCost(TokenUsage.EstimatedCost)}";
        }

        if (!string.IsNullOrEmpty(TokenUsage.ModelName))
        {
            text += $"\nModel: {TokenUsage.ModelName}";
        }

        return text;
    }

    /// <summary>
    /// Formats a token count with thousands separator.
    /// </summary>
    protected static string FormatTokenCount(int count)
    {
        return count.ToString("N0");
    }

    /// <summary>
    /// Formats a cost value for display.
    /// </summary>
    protected string FormatCost(decimal? cost)
    {
        if (cost == null)
            return "N/A";

        // For very small costs, show more precision
        if (cost < 0.01m)
        {
            return $"{CurrencySymbol}{cost:N6}";
        }

        return $"{CurrencySymbol}{cost:N4}";
    }
}
