using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Manages persistence of conversation state across sessions.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a storage abstraction for conversations, allowing implementations
/// to use various backends (localStorage, IndexedDB, server-side database, etc.).
/// </para>
/// <para>
/// <strong>Blazor Server Implementation:</strong>
/// Typically stores conversations in a server-side database or cache, associated with user sessions.
/// </para>
/// <para>
/// <strong>Blazor WASM Implementation:</strong>
/// May use browser storage (localStorage, IndexedDB) for offline support, with optional
/// synchronization to a server-side store.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Loading a conversation history
/// var conversation = await stateManager.LoadConversationAsync(conversationId);
/// if (conversation != null)
/// {
///     foreach (var message in conversation.Messages)
///     {
///         // Restore chat history in UI
///     }
/// }
/// </code>
/// </example>
public interface IAgentStateManager
{
    /// <summary>
    /// Saves a conversation to persistent storage.
    /// </summary>
    /// <param name="conversation">The conversation to save.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the conversation is saved.</returns>
    /// <remarks>
    /// If a conversation with the same ID already exists, it will be overwritten.
    /// </remarks>
    Task SaveConversationAsync(Conversation conversation, CancellationToken ct = default);

    /// <summary>
    /// Loads a conversation from persistent storage.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation to load.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// The requested <see cref="Conversation"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Conversation?> LoadConversationAsync(string conversationId, CancellationToken ct = default);

    /// <summary>
    /// Lists metadata for all stored conversations.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="ConversationMetadata"/> for all stored conversations.</returns>
    /// <remarks>
    /// This method returns lightweight metadata without loading full message histories.
    /// Results are typically ordered by <see cref="ConversationMetadata.LastModifiedAt"/> descending.
    /// </remarks>
    Task<IReadOnlyList<ConversationMetadata>> ListConversationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes a conversation from persistent storage.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation to delete.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the conversation is deleted.</returns>
    /// <remarks>
    /// This operation is idempotent; deleting a non-existent conversation does not throw.
    /// </remarks>
    Task DeleteConversationAsync(string conversationId, CancellationToken ct = default);
}
