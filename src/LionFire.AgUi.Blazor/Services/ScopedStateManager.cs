using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Configuration;
using LionFire.AgUi.Blazor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// A scoped state manager that provides per-user isolation and automatic cleanup.
/// </summary>
/// <remarks>
/// This implementation wraps another <see cref="IAgentStateManager"/> and adds:
/// <list type="bullet">
/// <item>User/session scoping via a scope identifier</item>
/// <item>Automatic cleanup based on <see cref="StateManagerOptions"/></item>
/// <item>Quota enforcement for storage limits</item>
/// </list>
/// </remarks>
public sealed class ScopedStateManager : IAgentStateManager
{
    private readonly IAgentStateManager _innerManager;
    private readonly IOptions<StateManagerOptions> _options;
    private readonly ILogger<ScopedStateManager> _logger;
    private readonly string _scopeId;

    /// <summary>
    /// Initializes a new instance of <see cref="ScopedStateManager"/>.
    /// </summary>
    /// <param name="innerManager">The underlying state manager to wrap.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="scopeId">The scope identifier (user ID, session ID, etc.). If null, a new GUID is used.</param>
    public ScopedStateManager(
        IAgentStateManager innerManager,
        IOptions<StateManagerOptions> options,
        ILogger<ScopedStateManager> logger,
        string? scopeId = null)
    {
        _innerManager = innerManager ?? throw new ArgumentNullException(nameof(innerManager));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeId = scopeId ?? Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Gets the scope identifier for this manager.
    /// </summary>
    public string ScopeId => _scopeId;

    private string ScopedId(string conversationId) => $"{_scopeId}:{conversationId}";

    private string UnscopedId(string scopedId)
    {
        var prefix = $"{_scopeId}:";
        return scopedId.StartsWith(prefix) ? scopedId[prefix.Length..] : scopedId;
    }

    /// <inheritdoc />
    public async Task SaveConversationAsync(Conversation conversation, CancellationToken ct = default)
    {
        var settings = _options.Value;

        // Check and enforce limits
        var existing = await _innerManager.ListConversationsAsync(ct);
        var scopedConversations = existing.Where(m => m.Id.StartsWith($"{_scopeId}:")).ToList();

        if (scopedConversations.Count >= settings.MaxConversations)
        {
            await EnforceCleanupPolicy(scopedConversations, settings, ct);
        }

        // Create a conversation with scoped ID
        var scopedConversation = conversation with { Id = ScopedId(conversation.Id) };
        await _innerManager.SaveConversationAsync(scopedConversation, ct);

        _logger.LogDebug("Saved conversation {ConversationId} for scope {ScopeId}",
            conversation.Id, _scopeId);
    }

    /// <inheritdoc />
    public async Task<Conversation?> LoadConversationAsync(string conversationId, CancellationToken ct = default)
    {
        var scopedId = ScopedId(conversationId);
        var conversation = await _innerManager.LoadConversationAsync(scopedId, ct);

        if (conversation == null)
            return null;

        // Return with original (unscoped) ID
        return conversation with { Id = UnscopedId(conversation.Id) };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConversationMetadata>> ListConversationsAsync(CancellationToken ct = default)
    {
        var all = await _innerManager.ListConversationsAsync(ct);
        var prefix = $"{_scopeId}:";

        return all
            .Where(m => m.Id.StartsWith(prefix))
            .Select(m => m with { Id = UnscopedId(m.Id) })
            .ToList();
    }

    /// <inheritdoc />
    public Task DeleteConversationAsync(string conversationId, CancellationToken ct = default)
    {
        var scopedId = ScopedId(conversationId);
        return _innerManager.DeleteConversationAsync(scopedId, ct);
    }

    private async Task EnforceCleanupPolicy(
        List<ConversationMetadata> conversations,
        StateManagerOptions settings,
        CancellationToken ct)
    {
        if (settings.CleanupPolicy == CleanupPolicy.None)
        {
            throw new InvalidOperationException(
                $"Maximum conversation limit ({settings.MaxConversations}) reached and cleanup policy is None.");
        }

        var toDelete = settings.CleanupPolicy switch
        {
            CleanupPolicy.OldestFirst => conversations
                .OrderBy(c => c.LastModifiedAt)
                .Take(1)
                .ToList(),
            CleanupPolicy.SmallestFirst => conversations
                .OrderBy(c => c.MessageCount)
                .Take(1)
                .ToList(),
            CleanupPolicy.LeastRecentlyUsed => conversations
                .OrderBy(c => c.LastModifiedAt)
                .Take(1)
                .ToList(),
            _ => []
        };

        foreach (var meta in toDelete)
        {
            _logger.LogInformation("Cleaning up conversation {ConversationId} due to {Policy} policy",
                meta.Id, settings.CleanupPolicy);
            await _innerManager.DeleteConversationAsync(meta.Id, ct);
        }
    }

    /// <summary>
    /// Cleans up old conversations based on age.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The number of conversations deleted.</returns>
    public async Task<int> CleanupOldConversationsAsync(CancellationToken ct = default)
    {
        var settings = _options.Value;
        var cutoff = DateTimeOffset.UtcNow - settings.MaxConversationAge;

        var all = await _innerManager.ListConversationsAsync(ct);
        var prefix = $"{_scopeId}:";

        var toDelete = all
            .Where(m => m.Id.StartsWith(prefix) && m.LastModifiedAt < cutoff)
            .ToList();

        foreach (var meta in toDelete)
        {
            await _innerManager.DeleteConversationAsync(meta.Id, ct);
        }

        if (toDelete.Count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} old conversations for scope {ScopeId}",
                toDelete.Count, _scopeId);
        }

        return toDelete.Count;
    }
}
