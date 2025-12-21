namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents a message that is pending delivery while streaming is in progress.
/// Unlike <see cref="Abstractions.QueuedMessage"/> which is for persistent offline queuing,
/// this is a lightweight in-memory structure for messages waiting to be sent during
/// an active chat session.
/// </summary>
public class PendingMessage
{
    /// <summary>
    /// Creates a new pending message.
    /// </summary>
    /// <param name="content">The text content of the message.</param>
    public PendingMessage(string content)
    {
        Content = content;
        QueuedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the text content of the message.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets when the message was queued.
    /// </summary>
    public DateTimeOffset QueuedAt { get; }
}
