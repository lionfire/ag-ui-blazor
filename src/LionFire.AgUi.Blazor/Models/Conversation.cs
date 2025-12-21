using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents a conversation between a user and an agent, including all messages exchanged.
/// </summary>
/// <param name="Id">The unique identifier for this conversation.</param>
/// <param name="AgentName">The name of the agent participating in this conversation.</param>
/// <param name="Messages">The list of messages exchanged in this conversation.</param>
/// <param name="CreatedAt">The timestamp when the conversation was created.</param>
/// <param name="LastModifiedAt">The timestamp when the conversation was last modified.</param>
/// <param name="Metadata">Optional metadata associated with the conversation.</param>
public sealed record Conversation(
    string Id,
    string AgentName,
    IReadOnlyList<ChatMessage> Messages,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastModifiedAt,
    IReadOnlyDictionary<string, object>? Metadata = null
)
{
    /// <summary>
    /// Creates a new conversation with the specified agent.
    /// </summary>
    /// <param name="agentName">The name of the agent for this conversation.</param>
    /// <returns>A new <see cref="Conversation"/> instance.</returns>
    public static Conversation Create(string agentName)
    {
        var now = DateTimeOffset.UtcNow;
        return new Conversation(
            Id: Guid.NewGuid().ToString(),
            AgentName: agentName,
            Messages: new List<ChatMessage>(),
            CreatedAt: now,
            LastModifiedAt: now
        );
    }

    /// <summary>
    /// Creates a new conversation with an added message.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <returns>A new <see cref="Conversation"/> instance with the message added.</returns>
    public Conversation WithMessage(ChatMessage message)
    {
        var newMessages = new List<ChatMessage>(Messages) { message };
        return this with
        {
            Messages = newMessages,
            LastModifiedAt = DateTimeOffset.UtcNow
        };
    }
}
