namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents a message operation request.
/// </summary>
public enum MessageOperationType
{
    /// <summary>
    /// Edit the message content.
    /// </summary>
    Edit,

    /// <summary>
    /// Regenerate the response from this point.
    /// </summary>
    Regenerate,

    /// <summary>
    /// Copy the message content to clipboard.
    /// </summary>
    Copy,

    /// <summary>
    /// Delete the message.
    /// </summary>
    Delete
}

/// <summary>
/// Event arguments for message operation requests.
/// </summary>
public sealed class MessageOperationEventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="MessageOperationEventArgs"/>.
    /// </summary>
    /// <param name="messageIndex">The index of the message in the conversation.</param>
    /// <param name="operation">The requested operation.</param>
    /// <param name="newContent">New content for edit operations, null otherwise.</param>
    public MessageOperationEventArgs(int messageIndex, MessageOperationType operation, string? newContent = null)
    {
        MessageIndex = messageIndex;
        Operation = operation;
        NewContent = newContent;
    }

    /// <summary>
    /// Gets the index of the message in the conversation.
    /// </summary>
    public int MessageIndex { get; }

    /// <summary>
    /// Gets the type of operation requested.
    /// </summary>
    public MessageOperationType Operation { get; }

    /// <summary>
    /// Gets the new content for edit operations.
    /// </summary>
    public string? NewContent { get; }
}
