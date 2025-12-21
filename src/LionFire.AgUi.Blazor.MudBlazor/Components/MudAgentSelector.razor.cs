using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A dropdown component for selecting an AI agent from a list.
/// </summary>
public partial class MudAgentSelector : ComponentBase
{
    #region MudBlazor Constants

    protected static string IconAgent => Icons.Material.Filled.SmartToy;
    protected static Size SmallSize => Size.Small;
    protected static Color PrimaryColor => Color.Primary;
    protected static Color TertiaryColor => Color.Tertiary;
    protected static global::MudBlazor.Variant SelectVariant => global::MudBlazor.Variant.Outlined;
    protected static Margin DenseMargin => Margin.Dense;
    protected static Adornment StartAdornment => Adornment.Start;
    protected static Typo TypoBody2 => Typo.body2;
    protected static Typo TypoCaption => Typo.caption;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the list of available agents.
    /// </summary>
    [Parameter]
    public IReadOnlyList<AgentInfo> Agents { get; set; } = Array.Empty<AgentInfo>();

    /// <summary>
    /// Gets or sets the currently selected agent name.
    /// </summary>
    [Parameter]
    public string? SelectedAgentName { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected agent changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> SelectedAgentNameChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when an agent is selected.
    /// </summary>
    [Parameter]
    public EventCallback<AgentInfo?> OnAgentSelected { get; set; }

    /// <summary>
    /// Gets or sets the label for the selector.
    /// </summary>
    [Parameter]
    public string Label { get; set; } = "Select Agent";

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Choose an agent...";

    /// <summary>
    /// Gets or sets whether the selector is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the selector is read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

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

    private static string GetAgentInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1)
        {
            return name.Length >= 2 ? name[..2].ToUpperInvariant() : name.ToUpperInvariant();
        }

        return $"{words[0][0]}{words[1][0]}".ToUpperInvariant();
    }

    private async Task HandleAgentChangedAsync(string? agentName)
    {
        if (SelectedAgentNameChanged.HasDelegate)
        {
            await SelectedAgentNameChanged.InvokeAsync(agentName);
        }

        if (OnAgentSelected.HasDelegate)
        {
            var agent = string.IsNullOrEmpty(agentName)
                ? null
                : Agents.FirstOrDefault(a => a.Name == agentName);
            await OnAgentSelected.InvokeAsync(agent);
        }
    }

    #endregion
}

/// <summary>
/// Represents information about an AI agent.
/// </summary>
/// <param name="Name">The unique name of the agent.</param>
/// <param name="Description">An optional description of the agent.</param>
/// <param name="IconUrl">An optional URL to the agent's icon.</param>
public sealed record AgentInfo(
    string Name,
    string? Description = null,
    string? IconUrl = null
);
