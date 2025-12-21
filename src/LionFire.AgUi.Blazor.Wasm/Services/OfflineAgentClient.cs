using System.Runtime.CompilerServices;
using LionFire.AgUi.Blazor.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LionFire.AgUi.Blazor.Wasm.Services;

/// <summary>
/// A wrapper around IChatClient that provides offline support by queuing messages when disconnected.
/// </summary>
/// <remarks>
/// <para>
/// When the connection monitor reports offline status, messages are queued for later delivery.
/// When connectivity is restored, queued messages can be replayed in order.
/// </para>
/// </remarks>
public sealed class OfflineAgentClient : IChatClient
{
    private readonly IChatClient _innerClient;
    private readonly IConnectionMonitor _connectionMonitor;
    private readonly IOfflineMessageQueue _messageQueue;
    private readonly string _agentName;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OfflineAgentClient"/>.
    /// </summary>
    /// <param name="innerClient">The underlying chat client.</param>
    /// <param name="connectionMonitor">The connection monitor.</param>
    /// <param name="messageQueue">The offline message queue.</param>
    /// <param name="agentName">The name of the agent.</param>
    /// <param name="logger">The logger instance.</param>
    public OfflineAgentClient(
        IChatClient innerClient,
        IConnectionMonitor connectionMonitor,
        IOfflineMessageQueue messageQueue,
        string agentName,
        ILogger logger)
    {
        _innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        _connectionMonitor = connectionMonitor ?? throw new ArgumentNullException(nameof(connectionMonitor));
        _messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
        _agentName = agentName ?? throw new ArgumentNullException(nameof(agentName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public ChatClientMetadata Metadata => _innerClient.Metadata;

    /// <inheritdoc />
    public async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_connectionMonitor.IsOnline)
        {
            try
            {
                // First, try to send any queued messages
                await ProcessQueuedMessagesAsync(cancellationToken);

                // Then send the current message
                return await _innerClient.CompleteAsync(chatMessages, options, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Request failed, queueing message for offline delivery");
                await QueueMessageAsync(chatMessages, cancellationToken);
                throw new OfflineException("Message queued for later delivery due to connection failure", ex);
            }
        }
        else
        {
            _logger.LogInformation("Client is offline, queueing message for later delivery");
            await QueueMessageAsync(chatMessages, cancellationToken);
            throw new OfflineException("Message queued for later delivery - client is offline");
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_connectionMonitor.IsOnline)
        {
            _logger.LogWarning("Streaming not supported when offline. Queueing message.");
            await QueueMessageAsync(chatMessages, cancellationToken);
            throw new OfflineException("Streaming requires an active connection - message queued for later delivery");
        }

        // First, try to send any queued messages
        await ProcessQueuedMessagesAsync(cancellationToken);

        // Then stream the current message
        await foreach (var update in _innerClient.CompleteStreamingAsync(chatMessages, options, cancellationToken))
        {
            yield return update;
        }
    }

    /// <inheritdoc />
    public TService? GetService<TService>(object? key = null) where TService : class
    {
        if (typeof(TService) == typeof(OfflineAgentClient))
            return this as TService;

        return _innerClient.GetService<TService>(key);
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? key = null)
    {
        if (serviceType == typeof(OfflineAgentClient))
            return this;

        return _innerClient.GetService(serviceType, key);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _innerClient.Dispose();
    }

    /// <summary>
    /// Processes all queued messages in order.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when all queued messages are processed.</returns>
    public async Task ProcessQueuedMessagesAsync(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            var message = await _messageQueue.PeekAsync(ct);
            if (message == null)
                break;

            if (message.AgentName != _agentName)
            {
                // Skip messages for other agents
                await _messageQueue.TryDequeueAsync(ct);
                continue;
            }

            try
            {
                _logger.LogDebug("Processing queued message {MessageId}", message.Id);
                await _innerClient.CompleteAsync(message.Messages, null, ct);
                await _messageQueue.MarkDeliveredAsync(message.Id, ct);
                _logger.LogInformation("Successfully delivered queued message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deliver queued message {MessageId}", message.Id);
                await _messageQueue.MarkFailedAsync(message.Id, ex.Message, ct);

                // If the message is expired, it will be removed automatically
                var updated = await _messageQueue.PeekAsync(ct);
                if (updated?.Id == message.Id && updated.IsExpired)
                {
                    await _messageQueue.TryDequeueAsync(ct);
                }
                break; // Stop processing on error
            }
        }
    }

    private async Task QueueMessageAsync(IList<ChatMessage> chatMessages, CancellationToken ct)
    {
        var queuedMessage = new QueuedMessage
        {
            AgentName = _agentName,
            Messages = chatMessages.ToList(),
            QueuedAt = DateTimeOffset.UtcNow
        };

        await _messageQueue.EnqueueAsync(queuedMessage, ct);
    }
}

/// <summary>
/// Exception thrown when an operation cannot be completed due to offline status.
/// </summary>
public class OfflineException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="OfflineException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    public OfflineException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OfflineException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public OfflineException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
