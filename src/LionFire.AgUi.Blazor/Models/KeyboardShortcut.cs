namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents a keyboard shortcut with its key combination and action.
/// </summary>
/// <param name="Key">The main key (e.g., "Enter", "K", "/").</param>
/// <param name="Modifiers">The modifier keys required (e.g., "Ctrl", "Cmd", "Shift").</param>
/// <param name="Action">The identifier for the action to perform.</param>
/// <param name="Description">Human-readable description of what the shortcut does.</param>
/// <param name="Category">Category for grouping shortcuts (e.g., "Messages", "Navigation").</param>
public sealed record KeyboardShortcut(
    string Key,
    KeyModifiers Modifiers,
    string Action,
    string Description,
    string Category = "General")
{
    /// <summary>
    /// Gets the display string for this shortcut (e.g., "Ctrl+Enter" or "⌘+K").
    /// </summary>
    /// <param name="useMacSymbols">If true, uses Mac-style symbols (⌘, ⌥, ⇧); otherwise uses text (Ctrl, Alt, Shift).</param>
    /// <returns>The formatted shortcut string.</returns>
    public string GetDisplayString(bool useMacSymbols = false)
    {
        var parts = new List<string>();

        if (Modifiers.HasFlag(KeyModifiers.Control))
        {
            parts.Add(useMacSymbols ? "⌃" : "Ctrl");
        }

        if (Modifiers.HasFlag(KeyModifiers.Meta))
        {
            parts.Add(useMacSymbols ? "⌘" : "Cmd");
        }

        if (Modifiers.HasFlag(KeyModifiers.Alt))
        {
            parts.Add(useMacSymbols ? "⌥" : "Alt");
        }

        if (Modifiers.HasFlag(KeyModifiers.Shift))
        {
            parts.Add(useMacSymbols ? "⇧" : "Shift");
        }

        parts.Add(FormatKey(Key));

        return string.Join(useMacSymbols ? "" : "+", parts);
    }

    private static string FormatKey(string key) => key switch
    {
        "Enter" => "↵",
        "Escape" => "Esc",
        "ArrowUp" => "↑",
        "ArrowDown" => "↓",
        "ArrowLeft" => "←",
        "ArrowRight" => "→",
        _ => key.ToUpperInvariant()
    };

    /// <summary>
    /// Checks if this shortcut matches the given key event.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <param name="ctrlKey">Whether Ctrl/Cmd was pressed.</param>
    /// <param name="altKey">Whether Alt was pressed.</param>
    /// <param name="shiftKey">Whether Shift was pressed.</param>
    /// <param name="metaKey">Whether Meta (Cmd on Mac) was pressed.</param>
    /// <returns>True if the shortcut matches the key event.</returns>
    public bool Matches(string key, bool ctrlKey, bool altKey, bool shiftKey, bool metaKey)
    {
        // Check key (case-insensitive)
        if (!string.Equals(Key, key, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check modifiers
        bool expectCtrl = Modifiers.HasFlag(KeyModifiers.Control);
        bool expectMeta = Modifiers.HasFlag(KeyModifiers.Meta);
        bool expectAlt = Modifiers.HasFlag(KeyModifiers.Alt);
        bool expectShift = Modifiers.HasFlag(KeyModifiers.Shift);

        // For cross-platform, Ctrl on Windows = Cmd on Mac
        // So we check if either ctrl or meta matches the expected control modifier
        bool ctrlOrMetaMatches = (ctrlKey || metaKey) == (expectCtrl || expectMeta);

        return ctrlOrMetaMatches &&
               altKey == expectAlt &&
               shiftKey == expectShift;
    }
}

/// <summary>
/// Modifier keys for keyboard shortcuts.
/// </summary>
[Flags]
public enum KeyModifiers
{
    /// <summary>No modifiers.</summary>
    None = 0,

    /// <summary>Control key (Ctrl on Windows/Linux).</summary>
    Control = 1,

    /// <summary>Alt key (Option on Mac).</summary>
    Alt = 2,

    /// <summary>Shift key.</summary>
    Shift = 4,

    /// <summary>Meta key (Windows key on Windows, Cmd on Mac).</summary>
    Meta = 8
}
