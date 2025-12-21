namespace LionFire.AgUi.Blazor.ErrorHandling;

/// <summary>
/// Represents an error that occurred during agent communication.
/// </summary>
/// <param name="Category">The category of error.</param>
/// <param name="Message">A user-friendly error message.</param>
/// <param name="TechnicalDetails">Technical details for logging/debugging.</param>
/// <param name="IsRetryable">Whether the operation can be retried.</param>
/// <param name="RetryAfter">If set, the recommended time to wait before retrying.</param>
/// <param name="ActionableGuidance">Guidance for the user on how to resolve the error.</param>
/// <param name="Exception">The underlying exception, if any.</param>
/// <param name="HttpStatusCode">The HTTP status code, if applicable.</param>
/// <param name="OccurredAt">When the error occurred.</param>
public sealed record AgentError(
    AgentErrorCategory Category,
    string Message,
    string? TechnicalDetails,
    bool IsRetryable,
    TimeSpan? RetryAfter,
    string? ActionableGuidance,
    Exception? Exception,
    int? HttpStatusCode,
    DateTimeOffset OccurredAt
)
{
    /// <summary>
    /// Creates an AgentError from an exception.
    /// </summary>
    public static AgentError FromException(Exception exception)
    {
        return AgentErrorClassifier.Classify(exception);
    }

    /// <summary>
    /// Creates an AgentError from an HTTP response.
    /// </summary>
    public static AgentError FromHttpResponse(int statusCode, string? responseBody = null)
    {
        return AgentErrorClassifier.ClassifyHttpStatus(statusCode, responseBody);
    }

    /// <summary>
    /// Creates a generic error.
    /// </summary>
    public static AgentError Create(
        AgentErrorCategory category,
        string message,
        bool isRetryable = false,
        string? actionableGuidance = null,
        Exception? exception = null)
    {
        return new AgentError(
            Category: category,
            Message: message,
            TechnicalDetails: exception?.ToString(),
            IsRetryable: isRetryable,
            RetryAfter: null,
            ActionableGuidance: actionableGuidance,
            Exception: exception,
            HttpStatusCode: null,
            OccurredAt: DateTimeOffset.UtcNow);
    }
}
