using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// View model for chat UI components, containing the current state of a chat session.
/// </summary>
/// <remarks>
/// This is a mutable class designed for use with Blazor component state management.
/// For immutable data, use <see cref="Conversation"/> instead.
/// </remarks>
public class ChatViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the current conversation.
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of messages in the conversation.
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets whether a response is currently being streamed.
    /// </summary>
    public bool IsStreaming { get; set; }

    /// <summary>
    /// Gets or sets the current connection state to the agent.
    /// </summary>
    public ConnectionState ConnectionState { get; set; } = ConnectionState.Disconnected;

    /// <summary>
    /// Gets or sets the list of tool calls pending user approval.
    /// </summary>
    public List<ToolCall> PendingToolCalls { get; set; } = new();

    /// <summary>
    /// Gets or sets the current error message, if any.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the cumulative token usage for this conversation.
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Gets whether there are any pending tool calls awaiting approval.
    /// </summary>
    public bool HasPendingToolCalls => PendingToolCalls.Count > 0;

    /// <summary>
    /// Gets whether there is an active error.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Gets whether the chat is in a ready state to accept user input.
    /// </summary>
    public bool IsReady => ConnectionState == ConnectionState.Connected && !IsStreaming && !HasPendingToolCalls;

    /// <summary>
    /// Clears any error message.
    /// </summary>
    public void ClearError()
    {
        ErrorMessage = null;
    }

    /// <summary>
    /// Resets the view model to its initial state.
    /// </summary>
    public void Reset()
    {
        ConversationId = string.Empty;
        Messages.Clear();
        IsStreaming = false;
        ConnectionState = ConnectionState.Disconnected;
        PendingToolCalls.Clear();
        ErrorMessage = null;
        TokenUsage = null;
    }

    /// <summary>
    /// Creates a snapshot of the current state as an immutable <see cref="Conversation"/>.
    /// </summary>
    /// <param name="agentName">The name of the agent.</param>
    /// <returns>An immutable <see cref="Conversation"/> representing the current state.</returns>
    public Conversation ToConversation(string agentName)
    {
        return new Conversation(
            Id: ConversationId,
            AgentName: agentName,
            Messages: Messages.ToList(),
            CreatedAt: DateTimeOffset.UtcNow, // Note: Original creation time not tracked in ViewModel
            LastModifiedAt: DateTimeOffset.UtcNow,
            Metadata: null
        );
    }
}
