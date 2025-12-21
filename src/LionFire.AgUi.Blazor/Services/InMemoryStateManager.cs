using System.Collections.Concurrent;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// In-memory implementation of <see cref="IAgentStateManager"/> for Blazor Server applications.
/// </summary>
/// <remarks>
/// <para>
/// This implementation stores conversations in memory using a concurrent dictionary.
/// It is suitable for development, testing, and scenarios where persistence across
/// application restarts is not required.
/// </para>
/// <para>
/// For production scenarios requiring persistence, consider using a database-backed
/// implementation or the LocalStorage implementation for WASM.
/// </para>
/// </remarks>
public sealed class InMemoryStateManager : IAgentStateManager
{
    private readonly ConcurrentDictionary<string, Conversation> _conversations = new();
    private readonly ConcurrentDictionary<string, ConversationMetadata> _metadata = new();

    /// <inheritdoc />
    public Task SaveConversationAsync(Conversation conversation, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        _conversations[conversation.Id] = conversation;
        _metadata[conversation.Id] = ConversationMetadata.FromConversation(conversation);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Conversation?> LoadConversationAsync(string conversationId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        _conversations.TryGetValue(conversationId, out var conversation);
        return Task.FromResult(conversation);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ConversationMetadata>> ListConversationsAsync(CancellationToken ct = default)
    {
        var result = _metadata.Values
            .OrderByDescending(m => m.LastModifiedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<ConversationMetadata>>(result);
    }

    /// <inheritdoc />
    public Task DeleteConversationAsync(string conversationId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        _conversations.TryRemove(conversationId, out _);
        _metadata.TryRemove(conversationId, out _);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears all stored conversations.
    /// </summary>
    /// <remarks>
    /// This method is primarily useful for testing scenarios.
    /// </remarks>
    public void Clear()
    {
        _conversations.Clear();
        _metadata.Clear();
    }

    /// <summary>
    /// Gets the total number of stored conversations.
    /// </summary>
    public int Count => _conversations.Count;
}
