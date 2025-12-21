using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Queue for storing messages when the client is offline.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a persistent queue for chat messages that could not be sent
/// due to network connectivity issues. Messages are stored until connectivity is restored
/// and can then be replayed.
/// </para>
/// <para>
/// <strong>Blazor WASM Implementation:</strong>
/// Uses browser LocalStorage or IndexedDB for persistence across page reloads.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Queue a message when offline
/// if (!connectionMonitor.IsOnline)
/// {
///     await offlineQueue.EnqueueAsync(queuedMessage);
/// }
///
/// // Process queue when back online
/// while (await offlineQueue.TryDequeueAsync() is QueuedMessage msg)
/// {
///     await ProcessMessageAsync(msg);
/// }
/// </code>
/// </example>
public interface IOfflineMessageQueue
{
    /// <summary>
    /// Enqueues a message for later delivery.
    /// </summary>
    /// <param name="message">The message to queue.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the message is queued.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the queue is full.</exception>
    Task EnqueueAsync(QueuedMessage message, CancellationToken ct = default);

    /// <summary>
    /// Attempts to dequeue the next message.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>The next queued message, or null if the queue is empty.</returns>
    Task<QueuedMessage?> TryDequeueAsync(CancellationToken ct = default);

    /// <summary>
    /// Peeks at the next message without removing it.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>The next queued message, or null if the queue is empty.</returns>
    Task<QueuedMessage?> PeekAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all queued messages without removing them.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of all queued messages.</returns>
    Task<IReadOnlyList<QueuedMessage>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Clears all queued messages.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the queue is cleared.</returns>
    Task ClearAsync(CancellationToken ct = default);

    /// <summary>
    /// Marks a message as successfully delivered and removes it from the queue.
    /// </summary>
    /// <param name="messageId">The ID of the message to mark as delivered.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the message is removed.</returns>
    Task MarkDeliveredAsync(string messageId, CancellationToken ct = default);

    /// <summary>
    /// Marks a message as failed delivery and updates retry count.
    /// </summary>
    /// <param name="messageId">The ID of the message that failed.</param>
    /// <param name="error">The error that occurred.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the message is updated.</returns>
    Task MarkFailedAsync(string messageId, string? error = null, CancellationToken ct = default);

    /// <summary>
    /// Gets the number of messages currently in the queue.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets whether the queue is empty.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Occurs when a message is added to the queue.
    /// </summary>
    event EventHandler<QueuedMessage>? MessageEnqueued;

    /// <summary>
    /// Occurs when a message is removed from the queue.
    /// </summary>
    event EventHandler<QueuedMessage>? MessageDequeued;
}

/// <summary>
/// Represents a message that has been queued for offline delivery.
/// </summary>
public sealed class QueuedMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for this queued message.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the target agent.
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation ID this message belongs to.
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// Gets or sets the chat messages to send.
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets when the message was queued.
    /// </summary>
    public DateTimeOffset QueuedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the last delivery attempt was made.
    /// </summary>
    public DateTimeOffset? LastAttemptAt { get; set; }

    /// <summary>
    /// Gets or sets the number of delivery attempts.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Gets or sets the last error message if delivery failed.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of delivery attempts.
    /// Default is 5.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Gets whether the message has exceeded max retry attempts.
    /// </summary>
    public bool IsExpired => AttemptCount >= MaxAttempts;
}
