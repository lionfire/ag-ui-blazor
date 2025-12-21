using System.Text.Json;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Wasm.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace LionFire.AgUi.Blazor.Wasm.Services;

/// <summary>
/// Offline message queue that persists to browser LocalStorage.
/// </summary>
/// <remarks>
/// <para>
/// Messages are stored in LocalStorage as JSON and persist across page reloads.
/// The queue automatically manages message expiration and retry counts.
/// </para>
/// </remarks>
public sealed class LocalStorageMessageQueue : IOfflineMessageQueue, IAsyncDisposable
{
    private const string StorageKey = "agui_offline_queue";

    private readonly IJSRuntime _jsRuntime;
    private readonly IOptions<WasmAgentClientOptions> _options;
    private readonly ILogger<LocalStorageMessageQueue> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    private List<QueuedMessage> _cache = new();
    private bool _isInitialized;
    private bool _isDisposed;

    /// <inheritdoc />
    public int Count => _cache.Count;

    /// <inheritdoc />
    public bool IsEmpty => _cache.Count == 0;

    /// <inheritdoc />
    public event EventHandler<QueuedMessage>? MessageEnqueued;

    /// <inheritdoc />
    public event EventHandler<QueuedMessage>? MessageDequeued;

    /// <summary>
    /// Initializes a new instance of <see cref="LocalStorageMessageQueue"/>.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public LocalStorageMessageQueue(
        IJSRuntime jsRuntime,
        IOptions<WasmAgentClientOptions> options,
        ILogger<LocalStorageMessageQueue> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Initializes the queue by loading persisted messages from LocalStorage.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_isInitialized || _isDisposed)
            return;

        await _lock.WaitAsync(ct);
        try
        {
            if (_isInitialized)
                return;

            await LoadFromStorageAsync(ct);
            _isInitialized = true;

            _logger.LogInformation("LocalStorageMessageQueue initialized with {Count} messages", _cache.Count);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(QueuedMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LocalStorageMessageQueue));

        await EnsureInitializedAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            var maxMessages = _options.Value.MaxQueuedMessages;
            if (_cache.Count >= maxMessages)
            {
                _logger.LogWarning("Offline queue is full ({MaxMessages} messages). Removing oldest message.", maxMessages);
                var oldest = _cache[0];
                _cache.RemoveAt(0);
                MessageDequeued?.Invoke(this, oldest);
            }

            _cache.Add(message);
            await SaveToStorageAsync(ct);

            _logger.LogDebug("Enqueued message {MessageId} for agent {AgentName}", message.Id, message.AgentName);
            MessageEnqueued?.Invoke(this, message);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<QueuedMessage?> TryDequeueAsync(CancellationToken ct = default)
    {
        if (_isDisposed)
            return null;

        await EnsureInitializedAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            if (_cache.Count == 0)
                return null;

            var message = _cache[0];
            _cache.RemoveAt(0);
            await SaveToStorageAsync(ct);

            _logger.LogDebug("Dequeued message {MessageId}", message.Id);
            MessageDequeued?.Invoke(this, message);

            return message;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<QueuedMessage?> PeekAsync(CancellationToken ct = default)
    {
        if (_isDisposed)
            return null;

        await EnsureInitializedAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            return _cache.Count > 0 ? _cache[0] : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<QueuedMessage>> GetAllAsync(CancellationToken ct = default)
    {
        if (_isDisposed)
            return Array.Empty<QueuedMessage>();

        await EnsureInitializedAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            return _cache.ToList().AsReadOnly();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken ct = default)
    {
        if (_isDisposed)
            return;

        await EnsureInitializedAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            var cleared = _cache.ToList();
            _cache.Clear();
            await SaveToStorageAsync(ct);

            foreach (var msg in cleared)
            {
                MessageDequeued?.Invoke(this, msg);
            }

            _logger.LogInformation("Cleared {Count} messages from offline queue", cleared.Count);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task MarkDeliveredAsync(string messageId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        if (_isDisposed)
            return;

        await EnsureInitializedAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            var message = _cache.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                _cache.Remove(message);
                await SaveToStorageAsync(ct);

                _logger.LogDebug("Marked message {MessageId} as delivered", messageId);
                MessageDequeued?.Invoke(this, message);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task MarkFailedAsync(string messageId, string? error = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        if (_isDisposed)
            return;

        await EnsureInitializedAsync(ct);

        await _lock.WaitAsync(ct);
        try
        {
            var message = _cache.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                message.AttemptCount++;
                message.LastAttemptAt = DateTimeOffset.UtcNow;
                message.LastError = error;

                if (message.IsExpired)
                {
                    _cache.Remove(message);
                    _logger.LogWarning(
                        "Message {MessageId} expired after {AttemptCount} attempts. Last error: {Error}",
                        messageId,
                        message.AttemptCount,
                        error);
                    MessageDequeued?.Invoke(this, message);
                }
                else
                {
                    _logger.LogDebug(
                        "Message {MessageId} failed (attempt {AttemptCount}/{MaxAttempts}): {Error}",
                        messageId,
                        message.AttemptCount,
                        message.MaxAttempts,
                        error);
                }

                await SaveToStorageAsync(ct);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (!_isInitialized)
        {
            await InitializeAsync(ct);
        }
    }

    private async Task LoadFromStorageAsync(CancellationToken ct)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem",
                ct,
                StorageKey);

            if (!string.IsNullOrEmpty(json))
            {
                var messages = JsonSerializer.Deserialize<List<QueuedMessageDto>>(json, _jsonOptions);
                if (messages != null)
                {
                    _cache = messages.Select(dto => dto.ToQueuedMessage()).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load offline queue from LocalStorage");
            _cache = new List<QueuedMessage>();
        }
    }

    private async Task SaveToStorageAsync(CancellationToken ct)
    {
        try
        {
            var dtos = _cache.Select(QueuedMessageDto.FromQueuedMessage).ToList();
            var json = JsonSerializer.Serialize(dtos, _jsonOptions);

            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                ct,
                StorageKey,
                json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save offline queue to LocalStorage");
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _lock.Dispose();

        await Task.CompletedTask;
    }

    /// <summary>
    /// DTO for serializing QueuedMessage to LocalStorage.
    /// </summary>
    /// <remarks>
    /// ChatMessage is complex to serialize, so we store simplified message content.
    /// </remarks>
    private sealed class QueuedMessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public List<MessageDto> Messages { get; set; } = new();
        public DateTimeOffset QueuedAt { get; set; }
        public DateTimeOffset? LastAttemptAt { get; set; }
        public int AttemptCount { get; set; }
        public string? LastError { get; set; }
        public int MaxAttempts { get; set; }

        public static QueuedMessageDto FromQueuedMessage(QueuedMessage msg)
        {
            return new QueuedMessageDto
            {
                Id = msg.Id,
                AgentName = msg.AgentName,
                ConversationId = msg.ConversationId,
                Messages = msg.Messages.Select(m => new MessageDto
                {
                    Role = m.Role.Value,
                    Content = m.Text ?? string.Empty
                }).ToList(),
                QueuedAt = msg.QueuedAt,
                LastAttemptAt = msg.LastAttemptAt,
                AttemptCount = msg.AttemptCount,
                LastError = msg.LastError,
                MaxAttempts = msg.MaxAttempts
            };
        }

        public QueuedMessage ToQueuedMessage()
        {
            return new QueuedMessage
            {
                Id = Id,
                AgentName = AgentName,
                ConversationId = ConversationId,
                Messages = Messages.Select(m => new Microsoft.Extensions.AI.ChatMessage(
                    ParseChatRole(m.Role),
                    m.Content)).ToList(),
                QueuedAt = QueuedAt,
                LastAttemptAt = LastAttemptAt,
                AttemptCount = AttemptCount,
                LastError = LastError,
                MaxAttempts = MaxAttempts
            };
        }

        private static Microsoft.Extensions.AI.ChatRole ParseChatRole(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "user" => Microsoft.Extensions.AI.ChatRole.User,
                "assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
                "system" => Microsoft.Extensions.AI.ChatRole.System,
                "tool" => Microsoft.Extensions.AI.ChatRole.Tool,
                _ => new Microsoft.Extensions.AI.ChatRole(value)
            };
        }
    }

    private sealed class MessageDto
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
