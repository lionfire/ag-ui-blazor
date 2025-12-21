namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Provides summary metadata about a conversation without including the full message history.
/// Useful for displaying conversation lists and search results.
/// </summary>
/// <param name="Id">The unique identifier for the conversation.</param>
/// <param name="AgentName">The name of the agent in this conversation.</param>
/// <param name="Title">An optional title or summary for the conversation.</param>
/// <param name="CreatedAt">The timestamp when the conversation was created.</param>
/// <param name="LastModifiedAt">The timestamp when the conversation was last modified.</param>
/// <param name="MessageCount">The total number of messages in the conversation.</param>
/// <param name="Tags">Tags associated with the conversation for categorization.</param>
public sealed record ConversationMetadata(
    string Id,
    string AgentName,
    string? Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastModifiedAt,
    int MessageCount,
    IReadOnlyList<string> Tags
)
{
    /// <summary>
    /// Creates metadata from a full conversation.
    /// </summary>
    /// <param name="conversation">The conversation to extract metadata from.</param>
    /// <param name="title">Optional title override.</param>
    /// <param name="tags">Optional tags to associate.</param>
    /// <returns>A new <see cref="ConversationMetadata"/> instance.</returns>
    public static ConversationMetadata FromConversation(
        Conversation conversation,
        string? title = null,
        IReadOnlyList<string>? tags = null)
    {
        return new ConversationMetadata(
            Id: conversation.Id,
            AgentName: conversation.AgentName,
            Title: title,
            CreatedAt: conversation.CreatedAt,
            LastModifiedAt: conversation.LastModifiedAt,
            MessageCount: conversation.Messages.Count,
            Tags: tags ?? Array.Empty<string>()
        );
    }
}
