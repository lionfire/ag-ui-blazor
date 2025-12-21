namespace LionFire.AgUi.Blazor.ErrorHandling;

/// <summary>
/// Categories of errors that can occur during agent communication.
/// </summary>
public enum AgentErrorCategory
{
    /// <summary>
    /// Unknown or unclassified error.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Network-related errors (connection refused, DNS failure, etc.).
    /// Transient - can be retried.
    /// </summary>
    Network,

    /// <summary>
    /// Request timeout.
    /// Transient - can be retried with longer timeout.
    /// </summary>
    Timeout,

    /// <summary>
    /// Rate limit exceeded (HTTP 429).
    /// Transient - should respect Retry-After header.
    /// </summary>
    RateLimit,

    /// <summary>
    /// Authentication or authorization error (HTTP 401, 403).
    /// Not retryable - requires user action.
    /// </summary>
    Authentication,

    /// <summary>
    /// Invalid request (HTTP 400).
    /// Not retryable - indicates programming error.
    /// </summary>
    BadRequest,

    /// <summary>
    /// Server error (HTTP 5xx).
    /// May be transient - can attempt limited retries.
    /// </summary>
    ServerError,

    /// <summary>
    /// Service unavailable (HTTP 503).
    /// Transient - server is temporarily overloaded.
    /// </summary>
    ServiceUnavailable,

    /// <summary>
    /// The operation was cancelled by the user.
    /// Not retryable.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Content policy violation or safety filter triggered.
    /// Not retryable with same content.
    /// </summary>
    ContentPolicy,

    /// <summary>
    /// Model not found or not available.
    /// Not retryable.
    /// </summary>
    ModelNotFound,

    /// <summary>
    /// Context length exceeded.
    /// Not retryable with same content.
    /// </summary>
    ContextLengthExceeded,

    /// <summary>
    /// Serialization or deserialization error.
    /// Not retryable.
    /// </summary>
    Serialization
}
