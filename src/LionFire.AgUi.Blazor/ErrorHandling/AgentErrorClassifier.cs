using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace LionFire.AgUi.Blazor.ErrorHandling;

/// <summary>
/// Classifies exceptions and HTTP responses into appropriate AgentError instances.
/// </summary>
public static class AgentErrorClassifier
{
    /// <summary>
    /// Classifies an exception into an AgentError.
    /// </summary>
    public static AgentError Classify(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Note: TaskCanceledException inherits from OperationCanceledException,
        // so we need to check the specific type first
        return exception switch
        {
            TaskCanceledException tce when tce.CancellationToken.IsCancellationRequested =>
                CreateCancelledError(tce),
            TaskCanceledException tce => CreateTimeoutError(tce),
            OperationCanceledException oce => CreateCancelledError(oce),
            TimeoutException te => CreateTimeoutError(te),
            HttpRequestException hre => ClassifyHttpException(hre),
            SocketException se => CreateNetworkError(se),
            System.Text.Json.JsonException je => CreateSerializationError(je),
            _ => CreateUnknownError(exception)
        };
    }

    /// <summary>
    /// Classifies an HTTP status code into an AgentError.
    /// </summary>
    public static AgentError ClassifyHttpStatus(int statusCode, string? responseBody = null)
    {
        return statusCode switch
        {
            400 => CreateBadRequestError(statusCode, responseBody),
            401 => CreateAuthenticationError(statusCode, "API key is invalid or expired"),
            403 => CreateAuthenticationError(statusCode, "Access denied - check your permissions"),
            404 => CreateModelNotFoundError(statusCode, responseBody),
            429 => CreateRateLimitError(statusCode, responseBody),
            500 => CreateServerError(statusCode, responseBody),
            502 => CreateServerError(statusCode, "Bad gateway - the server received an invalid response"),
            503 => CreateServiceUnavailableError(statusCode, responseBody),
            504 => CreateTimeoutError(statusCode, "Gateway timeout - the server took too long to respond"),
            >= 400 and < 500 => CreateBadRequestError(statusCode, responseBody),
            >= 500 => CreateServerError(statusCode, responseBody),
            _ => CreateUnknownHttpError(statusCode, responseBody)
        };
    }

    /// <summary>
    /// Extracts Retry-After duration from an HTTP response header value.
    /// </summary>
    public static TimeSpan? ParseRetryAfter(string? retryAfterHeader)
    {
        if (string.IsNullOrWhiteSpace(retryAfterHeader))
            return null;

        // Try parsing as seconds
        if (int.TryParse(retryAfterHeader, out var seconds))
            return TimeSpan.FromSeconds(seconds);

        // Try parsing as HTTP date
        if (DateTimeOffset.TryParse(retryAfterHeader, out var date))
        {
            var delay = date - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }

        return null;
    }

    private static AgentError CreateCancelledError(Exception exception)
    {
        return new AgentError(
            Category: AgentErrorCategory.Cancelled,
            Message: "The operation was cancelled.",
            TechnicalDetails: exception.Message,
            IsRetryable: false,
            RetryAfter: null,
            ActionableGuidance: null,
            Exception: exception,
            HttpStatusCode: null,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateTimeoutError(Exception exception)
    {
        return new AgentError(
            Category: AgentErrorCategory.Timeout,
            Message: "The request timed out. Please try again.",
            TechnicalDetails: exception.Message,
            IsRetryable: true,
            RetryAfter: TimeSpan.FromSeconds(5),
            ActionableGuidance: "Check your internet connection and try again. If the problem persists, the server may be experiencing high load.",
            Exception: exception,
            HttpStatusCode: null,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateTimeoutError(int statusCode, string message)
    {
        return new AgentError(
            Category: AgentErrorCategory.Timeout,
            Message: message,
            TechnicalDetails: $"HTTP {statusCode}",
            IsRetryable: true,
            RetryAfter: TimeSpan.FromSeconds(5),
            ActionableGuidance: "The server is taking too long to respond. Please try again.",
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError ClassifyHttpException(HttpRequestException exception)
    {
        // Check for specific status codes
        if (exception.StatusCode.HasValue)
        {
            return ClassifyHttpStatus((int)exception.StatusCode.Value, exception.Message);
        }

        // Network-level error
        return new AgentError(
            Category: AgentErrorCategory.Network,
            Message: "Unable to connect to the server. Please check your internet connection.",
            TechnicalDetails: exception.ToString(),
            IsRetryable: true,
            RetryAfter: TimeSpan.FromSeconds(2),
            ActionableGuidance: "Check your internet connection. If the problem persists, the server may be down.",
            Exception: exception,
            HttpStatusCode: null,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateNetworkError(SocketException exception)
    {
        return new AgentError(
            Category: AgentErrorCategory.Network,
            Message: "Network error occurred. Please check your internet connection.",
            TechnicalDetails: $"Socket error: {exception.SocketErrorCode} - {exception.Message}",
            IsRetryable: true,
            RetryAfter: TimeSpan.FromSeconds(2),
            ActionableGuidance: "Check your internet connection and try again.",
            Exception: exception,
            HttpStatusCode: null,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateSerializationError(System.Text.Json.JsonException exception)
    {
        return new AgentError(
            Category: AgentErrorCategory.Serialization,
            Message: "Failed to process the server response.",
            TechnicalDetails: exception.ToString(),
            IsRetryable: false,
            RetryAfter: null,
            ActionableGuidance: "This may indicate a compatibility issue. Please check for updates.",
            Exception: exception,
            HttpStatusCode: null,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateBadRequestError(int statusCode, string? responseBody)
    {
        var message = "Invalid request.";
        var guidance = "Please check your input and try again.";

        // Try to extract more specific error from response
        if (!string.IsNullOrEmpty(responseBody))
        {
            if (responseBody.Contains("context_length", StringComparison.OrdinalIgnoreCase) ||
                responseBody.Contains("token", StringComparison.OrdinalIgnoreCase))
            {
                return new AgentError(
                    Category: AgentErrorCategory.ContextLengthExceeded,
                    Message: "The conversation is too long. Please start a new conversation.",
                    TechnicalDetails: responseBody,
                    IsRetryable: false,
                    RetryAfter: null,
                    ActionableGuidance: "Clear the conversation history and try again with a shorter message.",
                    Exception: null,
                    HttpStatusCode: statusCode,
                    OccurredAt: DateTimeOffset.UtcNow);
            }

            if (responseBody.Contains("content_policy", StringComparison.OrdinalIgnoreCase) ||
                responseBody.Contains("safety", StringComparison.OrdinalIgnoreCase))
            {
                return new AgentError(
                    Category: AgentErrorCategory.ContentPolicy,
                    Message: "Your message was blocked by the content policy.",
                    TechnicalDetails: responseBody,
                    IsRetryable: false,
                    RetryAfter: null,
                    ActionableGuidance: "Please rephrase your message and try again.",
                    Exception: null,
                    HttpStatusCode: statusCode,
                    OccurredAt: DateTimeOffset.UtcNow);
            }
        }

        return new AgentError(
            Category: AgentErrorCategory.BadRequest,
            Message: message,
            TechnicalDetails: responseBody,
            IsRetryable: false,
            RetryAfter: null,
            ActionableGuidance: guidance,
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateAuthenticationError(int statusCode, string message)
    {
        return new AgentError(
            Category: AgentErrorCategory.Authentication,
            Message: message,
            TechnicalDetails: $"HTTP {statusCode}",
            IsRetryable: false,
            RetryAfter: null,
            ActionableGuidance: "Please check your API key in the settings and ensure it is valid.",
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateModelNotFoundError(int statusCode, string? responseBody)
    {
        return new AgentError(
            Category: AgentErrorCategory.ModelNotFound,
            Message: "The requested model is not available.",
            TechnicalDetails: responseBody,
            IsRetryable: false,
            RetryAfter: null,
            ActionableGuidance: "Please check the model name in your configuration and ensure it is supported.",
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateRateLimitError(int statusCode, string? responseBody)
    {
        // Try to parse retry-after from response body (some APIs include it there)
        TimeSpan? retryAfter = TimeSpan.FromSeconds(60); // Default fallback

        if (!string.IsNullOrEmpty(responseBody) && responseBody.Contains("retry", StringComparison.OrdinalIgnoreCase))
        {
            // Simple heuristic - look for numbers after "retry"
            var numbers = System.Text.RegularExpressions.Regex.Match(responseBody, @"(\d+)\s*(?:second|sec|s)?");
            if (numbers.Success && int.TryParse(numbers.Groups[1].Value, out var seconds))
            {
                retryAfter = TimeSpan.FromSeconds(seconds);
            }
        }

        return new AgentError(
            Category: AgentErrorCategory.RateLimit,
            Message: "You're sending requests too quickly. Please wait before trying again.",
            TechnicalDetails: responseBody,
            IsRetryable: true,
            RetryAfter: retryAfter,
            ActionableGuidance: $"Wait {retryAfter?.TotalSeconds:F0} seconds before sending another message.",
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateServerError(int statusCode, string? responseBody)
    {
        return new AgentError(
            Category: AgentErrorCategory.ServerError,
            Message: "The server encountered an error. Please try again.",
            TechnicalDetails: responseBody ?? $"HTTP {statusCode}",
            IsRetryable: true,
            RetryAfter: TimeSpan.FromSeconds(5),
            ActionableGuidance: "This is usually temporary. Please wait a moment and try again.",
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateServiceUnavailableError(int statusCode, string? responseBody)
    {
        return new AgentError(
            Category: AgentErrorCategory.ServiceUnavailable,
            Message: "The service is temporarily unavailable. Please try again later.",
            TechnicalDetails: responseBody,
            IsRetryable: true,
            RetryAfter: TimeSpan.FromSeconds(30),
            ActionableGuidance: "The server is experiencing high load. Please wait a moment and try again.",
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateUnknownError(Exception exception)
    {
        return new AgentError(
            Category: AgentErrorCategory.Unknown,
            Message: "An unexpected error occurred.",
            TechnicalDetails: exception.ToString(),
            IsRetryable: false,
            RetryAfter: null,
            ActionableGuidance: "Please try again. If the problem persists, contact support.",
            Exception: exception,
            HttpStatusCode: null,
            OccurredAt: DateTimeOffset.UtcNow);
    }

    private static AgentError CreateUnknownHttpError(int statusCode, string? responseBody)
    {
        return new AgentError(
            Category: AgentErrorCategory.Unknown,
            Message: $"Unexpected response from server (HTTP {statusCode}).",
            TechnicalDetails: responseBody,
            IsRetryable: false,
            RetryAfter: null,
            ActionableGuidance: "Please try again. If the problem persists, contact support.",
            Exception: null,
            HttpStatusCode: statusCode,
            OccurredAt: DateTimeOffset.UtcNow);
    }
}
