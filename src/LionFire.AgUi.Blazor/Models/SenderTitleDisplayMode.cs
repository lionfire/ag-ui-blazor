namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Controls when sender titles (e.g., "You", "Assistant") are displayed in chat messages.
/// </summary>
public enum SenderTitleDisplayMode
{
    /// <summary>
    /// Always show sender titles on all messages.
    /// </summary>
    Enabled,

    /// <summary>
    /// Never show sender titles.
    /// </summary>
    Disabled,

    /// <summary>
    /// Automatically determine when to show titles:
    /// - Always show for the current user ("You")
    /// - Show for non-user senders only if more than one distinct non-user sender is detected.
    /// This is useful for group chats where multiple assistants or participants may be present.
    /// </summary>
    Auto
}
