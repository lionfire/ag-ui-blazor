using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A dialog component that displays available keyboard shortcuts.
/// </summary>
public partial class MudKeyboardShortcutsHelp : ComponentBase
{
    #region Constants

    /// <summary>MudBlazor keyboard icon.</summary>
    protected static string KeyboardIcon => Icons.Material.Filled.Keyboard;

    /// <summary>MudBlazor default color.</summary>
    protected static Color DefaultColor => Color.Default;

    /// <summary>MudBlazor primary color.</summary>
    protected static Color PrimaryColor => Color.Primary;

    /// <summary>MudBlazor secondary color.</summary>
    protected static Color SecondaryColor => Color.Secondary;

    /// <summary>MudBlazor small size.</summary>
    protected static Size SmallSize => Size.Small;

    /// <summary>MudBlazor outlined variant.</summary>
    protected static Variant OutlinedVariant => Variant.Outlined;

    /// <summary>MudBlazor text variant.</summary>
    protected static Variant TextVariant => Variant.Text;

    /// <summary>MudBlazor h6 typography.</summary>
    protected static Typo TypoH6 => Typo.h6;

    /// <summary>MudBlazor subtitle1 typography.</summary>
    protected static Typo TypoSubtitle1 => Typo.subtitle1;

    /// <summary>MudBlazor caption typography.</summary>
    protected static Typo TypoCaption => Typo.caption;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the shortcuts grouped by category.
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<string, IReadOnlyList<KeyboardShortcut>>? ShortcutsByCategory { get; set; }

    /// <summary>
    /// Gets or sets whether the platform is Mac (for display purposes).
    /// </summary>
    [Parameter]
    public bool IsMac { get; set; }

    /// <summary>
    /// Gets or sets the MudDialog instance for closing.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    protected void Close()
    {
        MudDialog?.Close();
    }

    /// <summary>
    /// Gets the display order for a category.
    /// </summary>
    /// <param name="category">The category name.</param>
    /// <returns>The sort order.</returns>
    protected static int GetCategoryOrder(string category) => category switch
    {
        "Messages" => 0,
        "Chat" => 1,
        "Navigation" => 2,
        "Help" => 3,
        _ => 99
    };

    #endregion
}
