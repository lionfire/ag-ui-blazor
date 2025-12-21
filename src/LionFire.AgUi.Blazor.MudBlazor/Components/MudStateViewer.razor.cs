using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A component for viewing and inspecting state data.
/// </summary>
public partial class MudStateViewer : ComponentBase
{
    #region MudBlazor Constants

    protected static string IconRefresh => Icons.Material.Filled.Refresh;
    protected static string IconExpandAll => Icons.Material.Filled.UnfoldMore;
    protected static string IconCollapseAll => Icons.Material.Filled.UnfoldLess;
    protected static string IconChevronDown => Icons.Material.Filled.KeyboardArrowDown;
    protected static string IconChevronUp => Icons.Material.Filled.KeyboardArrowUp;
    protected static Size SmallSize => Size.Small;
    protected static Color TertiaryColor => Color.Tertiary;
    protected static Typo TypoSubtitle2 => Typo.subtitle2;
    protected static Typo TypoBody2 => Typo.body2;
    protected static Typo TypoCaption => Typo.caption;

    #endregion

    #region State

    private readonly HashSet<string> _expandedKeys = new();
    private bool AllExpanded => State.Count > 0 && _expandedKeys.Count == State.Count;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the title of the state viewer.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "State Viewer";

    /// <summary>
    /// Gets or sets the state data to display.
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<string, object?> State { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets or sets whether to show the refresh button.
    /// </summary>
    [Parameter]
    public bool ShowRefreshButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the expand/collapse all button.
    /// </summary>
    [Parameter]
    public bool ShowExpandAllButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the message to display when no state is present.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No state data available";

    /// <summary>
    /// Gets or sets the last updated timestamp.
    /// </summary>
    [Parameter]
    public string? LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the refresh button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnRefresh { get; set; }

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

    private bool IsExpanded(string key) => _expandedKeys.Contains(key);

    private void ToggleExpand(string key)
    {
        if (_expandedKeys.Contains(key))
        {
            _expandedKeys.Remove(key);
        }
        else
        {
            _expandedKeys.Add(key);
        }
    }

    private void ToggleExpandAll()
    {
        if (AllExpanded)
        {
            _expandedKeys.Clear();
        }
        else
        {
            foreach (var key in State.Keys)
            {
                _expandedKeys.Add(key);
            }
        }
    }

    private static bool IsExpandable(object? value)
    {
        if (value == null) return false;

        var type = value.GetType();
        return type.IsClass && type != typeof(string);
    }

    private static string GetValuePreview(object? value)
    {
        if (value == null) return "null";

        var type = value.GetType();
        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
        {
            var collection = value as System.Collections.ICollection;
            return $"[{collection?.Count ?? 0} items]";
        }

        if (type.IsClass && type != typeof(string))
        {
            return $"{{{type.Name}}}";
        }

        return FormatValue(value);
    }

    private static string FormatValue(object? value)
    {
        if (value == null) return "null";

        return value switch
        {
            string s => s,
            bool b => b ? "true" : "false",
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss"),
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            _ when value.GetType().IsClass && value.GetType() != typeof(string)
                => JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }),
            _ => value.ToString() ?? "null"
        };
    }

    private async Task HandleRefreshAsync()
    {
        if (OnRefresh.HasDelegate)
        {
            await OnRefresh.InvokeAsync();
        }
    }

    #endregion
}
