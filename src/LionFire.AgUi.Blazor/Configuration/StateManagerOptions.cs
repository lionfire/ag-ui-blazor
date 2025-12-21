namespace LionFire.AgUi.Blazor.Configuration;

/// <summary>
/// Configuration options for state manager implementations.
/// </summary>
public sealed class StateManagerOptions
{
    /// <summary>
    /// Gets or sets the maximum number of conversations to store.
    /// Default is 100.
    /// </summary>
    /// <remarks>
    /// When this limit is exceeded, the oldest conversations will be removed
    /// according to the <see cref="CleanupPolicy"/>.
    /// </remarks>
    public int MaxConversations { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum age for conversations before cleanup.
    /// Default is 30 days.
    /// </summary>
    public TimeSpan MaxConversationAge { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Gets or sets the maximum size in bytes for LocalStorage persistence.
    /// Default is 5MB (5,242,880 bytes).
    /// </summary>
    /// <remarks>
    /// This setting is primarily used by the LocalStorage implementation for WASM.
    /// Most browsers have a 5-10MB limit per origin.
    /// </remarks>
    public long MaxStorageSizeBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the cleanup policy for old conversations.
    /// Default is <see cref="CleanupPolicy.OldestFirst"/>.
    /// </summary>
    public CleanupPolicy CleanupPolicy { get; set; } = CleanupPolicy.OldestFirst;

    /// <summary>
    /// Gets or sets whether to automatically save conversations after each message.
    /// Default is true.
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// Gets or sets the debounce interval for auto-save operations.
    /// Default is 1 second.
    /// </summary>
    /// <remarks>
    /// This helps reduce storage operations during rapid message exchanges.
    /// </remarks>
    public TimeSpan AutoSaveDebounce { get; set; } = TimeSpan.FromSeconds(1);
}

/// <summary>
/// Defines policies for cleaning up old conversations.
/// </summary>
public enum CleanupPolicy
{
    /// <summary>
    /// Remove the oldest conversations first (by LastModifiedAt).
    /// </summary>
    OldestFirst,

    /// <summary>
    /// Remove the smallest conversations first (by message count).
    /// </summary>
    SmallestFirst,

    /// <summary>
    /// Remove the least recently accessed conversations first.
    /// </summary>
    LeastRecentlyUsed,

    /// <summary>
    /// Do not automatically clean up; throw when limits are exceeded.
    /// </summary>
    None
}
