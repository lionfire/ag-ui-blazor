using Microsoft.Extensions.Logging;

namespace LionFire.AgUi.Blazor.ErrorHandling;

/// <summary>
/// Configuration options for retry behavior.
/// </summary>
public sealed class RetryPolicyOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retries.
    /// Default is 1 second.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum delay between retries.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the backoff multiplier for exponential backoff.
    /// Default is 2.0 (each retry waits twice as long).
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets whether to add random jitter to delays.
    /// Default is true (helps prevent thundering herd).
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum jitter as a fraction of the delay.
    /// Default is 0.25 (25% jitter).
    /// </summary>
    public double JitterFraction { get; set; } = 0.25;

    /// <summary>
    /// Gets or sets whether to retry on server errors (5xx).
    /// Default is true with limited retries.
    /// </summary>
    public bool RetryServerErrors { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum retries for server errors.
    /// Default is 1 (one retry for 5xx errors).
    /// </summary>
    public int MaxServerErrorRetries { get; set; } = 1;
}

/// <summary>
/// Provides retry logic with exponential backoff for agent operations.
/// </summary>
public sealed class RetryPolicy
{
    private static readonly Random _jitterRandom = new();
    private readonly RetryPolicyOptions _options;
    private readonly ILogger<RetryPolicy>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RetryPolicy"/> with default options.
    /// </summary>
    public RetryPolicy()
        : this(new RetryPolicyOptions(), null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RetryPolicy"/>.
    /// </summary>
    public RetryPolicy(RetryPolicyOptions options, ILogger<RetryPolicy>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <summary>
    /// Gets the options for this retry policy.
    /// </summary>
    public RetryPolicyOptions Options => _options;

    /// <summary>
    /// Executes an operation with retry logic.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="AgentOperationException">Thrown when all retries are exhausted.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var context = new RetryContext(_options);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var error = AgentError.FromException(ex);
                context.RecordAttempt(error); // Record attempt first

                if (!ShouldRetry(error, context))
                {
                    _logger?.LogError(ex,
                        "Operation failed after {AttemptCount} attempts: {ErrorMessage}",
                        context.AttemptCount, error.Message);

                    throw new AgentOperationException(error, context.AttemptCount);
                }

                var delay = CalculateDelay(error, context);

                _logger?.LogWarning(
                    "Operation failed (attempt {AttemptCount}/{MaxAttempts}), retrying in {DelayMs}ms: {ErrorMessage}",
                    context.AttemptCount, context.MaxAttempts, delay.TotalMilliseconds, error.Message);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Executes an operation with retry logic.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="AgentOperationException">Thrown when all retries are exhausted.</exception>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async ct =>
        {
            await operation(ct).ConfigureAwait(false);
            return true;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines if an operation should be retried based on the error and context.
    /// </summary>
    public bool ShouldRetry(AgentError error, RetryContext context)
    {
        if (!error.IsRetryable)
            return false;

        if (context.AttemptCount >= context.MaxAttempts)
            return false;

        // Special handling for server errors
        if (error.Category == AgentErrorCategory.ServerError)
        {
            return _options.RetryServerErrors &&
                   context.ServerErrorCount < _options.MaxServerErrorRetries;
        }

        return true;
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt.
    /// </summary>
    public TimeSpan CalculateDelay(AgentError error, RetryContext context)
    {
        // Respect Retry-After header if present
        if (error.RetryAfter.HasValue)
        {
            return error.RetryAfter.Value;
        }

        // Calculate exponential backoff
        var baseDelay = _options.InitialDelay.TotalMilliseconds *
                        Math.Pow(_options.BackoffMultiplier, context.AttemptCount);

        // Cap at maximum delay
        var delay = Math.Min(baseDelay, _options.MaxDelay.TotalMilliseconds);

        // Add jitter if enabled
        if (_options.UseJitter)
        {
            var jitterRange = delay * _options.JitterFraction;
            var jitter = (_jitterRandom.NextDouble() * 2 - 1) * jitterRange;
            delay = Math.Max(0, delay + jitter);
        }

        return TimeSpan.FromMilliseconds(delay);
    }
}

/// <summary>
/// Tracks the state of retry attempts for an operation.
/// </summary>
public sealed class RetryContext
{
    private readonly RetryPolicyOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="RetryContext"/>.
    /// </summary>
    public RetryContext(RetryPolicyOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        MaxAttempts = options.MaxRetries + 1; // +1 for initial attempt
    }

    /// <summary>
    /// Gets the number of attempts made so far.
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// Gets the number of server errors encountered.
    /// </summary>
    public int ServerErrorCount { get; private set; }

    /// <summary>
    /// Gets the maximum number of attempts allowed.
    /// </summary>
    public int MaxAttempts { get; }

    /// <summary>
    /// Gets the list of errors encountered during retries.
    /// </summary>
    public List<AgentError> Errors { get; } = new();

    /// <summary>
    /// Records an attempt and its error.
    /// </summary>
    public void RecordAttempt(AgentError? error = null)
    {
        AttemptCount++;

        if (error != null)
        {
            Errors.Add(error);

            if (error.Category == AgentErrorCategory.ServerError)
            {
                ServerErrorCount++;
            }
        }
    }
}

/// <summary>
/// Exception thrown when an agent operation fails after all retries.
/// </summary>
public sealed class AgentOperationException : Exception
{
    /// <summary>
    /// Gets the error that caused the operation to fail.
    /// </summary>
    public AgentError Error { get; }

    /// <summary>
    /// Gets the number of attempts made before failing.
    /// </summary>
    public int AttemptCount { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="AgentOperationException"/>.
    /// </summary>
    public AgentOperationException(AgentError error, int attemptCount)
        : base(error.Message, error.Exception)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        AttemptCount = attemptCount;
    }
}
