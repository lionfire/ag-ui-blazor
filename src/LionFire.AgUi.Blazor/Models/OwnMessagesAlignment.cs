namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Controls the alignment of the current user's messages in the chat UI.
/// </summary>
public enum OwnMessagesAlignment
{
    /// <summary>
    /// User messages are aligned to the left, same as other participants.
    /// Useful for group chats or when a consistent layout is preferred.
    /// </summary>
    Left,

    /// <summary>
    /// User messages are aligned to the right (default).
    /// Provides visual distinction between the user's messages and others.
    /// </summary>
    Right
}
