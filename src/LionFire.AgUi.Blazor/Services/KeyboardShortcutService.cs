using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// Default implementation of <see cref="IKeyboardShortcutService"/>.
/// Manages keyboard shortcuts with platform-aware display strings.
/// </summary>
public sealed class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly List<KeyboardShortcut> _shortcuts = new();
    private readonly object _lock = new();
    private bool _isMac;

    /// <inheritdoc />
    public IReadOnlyList<KeyboardShortcut> Shortcuts
    {
        get
        {
            lock (_lock)
            {
                return _shortcuts.ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IReadOnlyList<KeyboardShortcut>> ShortcutsByCategory
    {
        get
        {
            lock (_lock)
            {
                return _shortcuts
                    .GroupBy(s => s.Category)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyList<KeyboardShortcut>)g.ToList().AsReadOnly());
            }
        }
    }

    /// <inheritdoc />
    public bool IsMac => _isMac;

    /// <inheritdoc />
    public void Register(KeyboardShortcut shortcut)
    {
        ArgumentNullException.ThrowIfNull(shortcut);

        lock (_lock)
        {
            // Remove existing shortcut with same action
            _shortcuts.RemoveAll(s => s.Action == shortcut.Action);
            _shortcuts.Add(shortcut);
        }
    }

    /// <inheritdoc />
    public void Unregister(string action)
    {
        lock (_lock)
        {
            _shortcuts.RemoveAll(s => s.Action == action);
        }
    }

    /// <inheritdoc />
    public KeyboardShortcut? FindMatch(string key, bool ctrlKey, bool altKey, bool shiftKey, bool metaKey)
    {
        lock (_lock)
        {
            return _shortcuts.FirstOrDefault(s => s.Matches(key, ctrlKey, altKey, shiftKey, metaKey));
        }
    }

    /// <inheritdoc />
    public string? GetDisplayString(string action)
    {
        lock (_lock)
        {
            var shortcut = _shortcuts.FirstOrDefault(s => s.Action == action);
            return shortcut?.GetDisplayString(_isMac);
        }
    }

    /// <inheritdoc />
    public void SetPlatform(bool isMac)
    {
        _isMac = isMac;
    }

    /// <summary>
    /// Registers the default set of keyboard shortcuts for the chat UI.
    /// </summary>
    public void RegisterDefaults()
    {
        // Messages category
        Register(new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send message", "Messages"));
        Register(new KeyboardShortcut("Enter", KeyModifiers.None, "send-inline", "Send message (when input focused)", "Messages"));

        // Chat category
        Register(new KeyboardShortcut("k", KeyModifiers.Control, "new-chat", "Start new chat", "Chat"));
        Register(new KeyboardShortcut("Escape", KeyModifiers.None, "cancel", "Cancel streaming / Close dialog", "Chat"));

        // Navigation category
        Register(new KeyboardShortcut("ArrowUp", KeyModifiers.Control, "previous-message", "Navigate to previous message", "Navigation"));
        Register(new KeyboardShortcut("ArrowDown", KeyModifiers.Control, "next-message", "Navigate to next message", "Navigation"));

        // Help category
        Register(new KeyboardShortcut("/", KeyModifiers.Control, "show-shortcuts", "Show keyboard shortcuts", "Help"));
        Register(new KeyboardShortcut("?", KeyModifiers.Control | KeyModifiers.Shift, "show-shortcuts", "Show keyboard shortcuts", "Help"));
    }
}
