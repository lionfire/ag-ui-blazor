using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Service for managing keyboard shortcuts in the application.
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Gets all registered keyboard shortcuts.
    /// </summary>
    IReadOnlyList<KeyboardShortcut> Shortcuts { get; }

    /// <summary>
    /// Gets shortcuts grouped by category.
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<KeyboardShortcut>> ShortcutsByCategory { get; }

    /// <summary>
    /// Gets whether the current platform is Mac (affects display strings).
    /// </summary>
    bool IsMac { get; }

    /// <summary>
    /// Registers a new keyboard shortcut.
    /// </summary>
    /// <param name="shortcut">The shortcut to register.</param>
    void Register(KeyboardShortcut shortcut);

    /// <summary>
    /// Unregisters a keyboard shortcut by its action name.
    /// </summary>
    /// <param name="action">The action identifier to unregister.</param>
    void Unregister(string action);

    /// <summary>
    /// Finds a shortcut that matches the given key event.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <param name="ctrlKey">Whether Ctrl was pressed.</param>
    /// <param name="altKey">Whether Alt was pressed.</param>
    /// <param name="shiftKey">Whether Shift was pressed.</param>
    /// <param name="metaKey">Whether Meta (Cmd) was pressed.</param>
    /// <returns>The matching shortcut, or null if none matches.</returns>
    KeyboardShortcut? FindMatch(string key, bool ctrlKey, bool altKey, bool shiftKey, bool metaKey);

    /// <summary>
    /// Gets the display string for a shortcut action.
    /// </summary>
    /// <param name="action">The action identifier.</param>
    /// <returns>The display string, or null if the action is not registered.</returns>
    string? GetDisplayString(string action);

    /// <summary>
    /// Sets the platform for display purposes. Called during initialization.
    /// </summary>
    /// <param name="isMac">True if running on Mac.</param>
    void SetPlatform(bool isMac);
}
