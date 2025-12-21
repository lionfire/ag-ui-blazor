using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A search input component for searching conversations.
/// </summary>
public partial class MudConversationSearch : ComponentBase
{
    #region MudBlazor Constants

    protected static string IconSearch => Icons.Material.Filled.Search;
    protected static string IconClear => Icons.Material.Filled.Clear;
    protected static Size SmallSize => Size.Small;
    protected static Color TertiaryColor => Color.Tertiary;
    protected static Color PrimaryColor => Color.Primary;
    protected static Adornment EndAdornment => Adornment.End;
    protected static global::MudBlazor.Variant TextFieldVariant => global::MudBlazor.Variant.Outlined;
    protected static Margin DenseMargin => Margin.Dense;

    #endregion

    #region State

    private string? _searchQuery;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the current search query.
    /// </summary>
    [Parameter]
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the search query changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> SearchQueryChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a search is triggered.
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnSearch { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Search conversations...";

    /// <summary>
    /// Gets or sets the debounce interval in milliseconds.
    /// Default is 300ms.
    /// </summary>
    [Parameter]
    public int DebounceMs { get; set; } = 300;

    /// <summary>
    /// Gets or sets whether to show the clear button.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowClearButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a search is currently in progress.
    /// </summary>
    [Parameter]
    public bool IsSearching { get; set; }

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

    #endregion

    #region Lifecycle

    protected override void OnParametersSet()
    {
        if (_searchQuery != SearchQuery)
        {
            _searchQuery = SearchQuery;
        }
    }

    #endregion

    #region Event Handlers

    private async Task HandleSearchAsync(string value)
    {
        _searchQuery = value;

        if (SearchQueryChanged.HasDelegate)
        {
            await SearchQueryChanged.InvokeAsync(_searchQuery);
        }

        if (OnSearch.HasDelegate)
        {
            await OnSearch.InvokeAsync(_searchQuery);
        }
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await ClearSearchAsync();
        }
    }

    private async Task ClearSearchAsync()
    {
        _searchQuery = null;

        if (SearchQueryChanged.HasDelegate)
        {
            await SearchQueryChanged.InvokeAsync(null);
        }

        if (OnSearch.HasDelegate)
        {
            await OnSearch.InvokeAsync(null);
        }
    }

    #endregion
}
