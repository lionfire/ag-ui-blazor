using LionFire.AgUi.Blazor.ErrorHandling;

namespace LionFire.AgUi.Blazor.Tests.ErrorHandling;

public class RetryPolicyTests
{
    private readonly RetryPolicyOptions _options;
    private readonly RetryPolicy _sut;

    public RetryPolicyTests()
    {
        _options = new RetryPolicyOptions
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(10),
            MaxDelay = TimeSpan.FromMilliseconds(100),
            UseJitter = false // Disable jitter for predictable tests
        };
        _sut = new RetryPolicy(_options);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessOnFirstAttempt_ReturnsResult()
    {
        // Arrange
        var callCount = 0;

        // Act
        var result = await _sut.ExecuteAsync(async ct =>
        {
            callCount++;
            await Task.Delay(1, ct);
            return 42;
        });

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessAfterRetry_ReturnsResult()
    {
        // Arrange
        var callCount = 0;

        // Act
        var result = await _sut.ExecuteAsync(async ct =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new TimeoutException("Simulated timeout");
            }
            await Task.Delay(1, ct);
            return 42;
        });

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_AllRetriesExhausted_ThrowsAgentOperationException()
    {
        // Arrange
        var callCount = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AgentOperationException>(async () =>
        {
            await _sut.ExecuteAsync(async ct =>
            {
                callCount++;
                await Task.Delay(1, ct);
                throw new TimeoutException("Simulated timeout");
            });
        });

        Assert.Equal(AgentErrorCategory.Timeout, exception.Error.Category);
        // MaxRetries = 3 means max 4 attempts total (1 initial + 3 retries)
        Assert.Equal(4, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_NonRetryableError_DoesNotRetry()
    {
        // Arrange
        var callCount = 0;

        // Act & Assert
        // Note: OperationCanceledException is not caught by the retry logic,
        // so we use a different non-retryable exception
        var exception = await Assert.ThrowsAsync<AgentOperationException>(async () =>
        {
            await _sut.ExecuteAsync(async ct =>
            {
                callCount++;
                await Task.Delay(1, ct);
                throw new System.Text.Json.JsonException("Serialization error"); // Not retryable
            });
        });

        Assert.Equal(AgentErrorCategory.Serialization, exception.Error.Category);
        Assert.Equal(1, callCount); // No retries
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ThrowsImmediately()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var callCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await _sut.ExecuteAsync(async ct =>
            {
                callCount++;
                await Task.Delay(1, ct);
                return 42;
            }, cts.Token);
        });

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void ShouldRetry_RetryableError_WithAttemptsRemaining_ReturnsTrue()
    {
        // Arrange
        var error = AgentError.Create(AgentErrorCategory.Timeout, "Timeout", isRetryable: true);
        var context = new RetryContext(_options);

        // Act
        var result = _sut.ShouldRetry(error, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldRetry_NonRetryableError_ReturnsFalse()
    {
        // Arrange
        var error = AgentError.Create(AgentErrorCategory.Authentication, "Auth failed", isRetryable: false);
        var context = new RetryContext(_options);

        // Act
        var result = _sut.ShouldRetry(error, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldRetry_MaxAttemptsReached_ReturnsFalse()
    {
        // Arrange
        var error = AgentError.Create(AgentErrorCategory.Timeout, "Timeout", isRetryable: true);
        var context = new RetryContext(_options);
        // MaxAttempts = MaxRetries + 1 = 4, so record 4 attempts to reach limit
        for (int i = 0; i < _options.MaxRetries + 1; i++)
        {
            context.RecordAttempt(error);
        }

        // Act
        var result = _sut.ShouldRetry(error, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldRetry_ServerError_WithMaxServerErrorsReached_ReturnsFalse()
    {
        // Arrange
        _options.MaxServerErrorRetries = 1;
        var error = AgentError.Create(AgentErrorCategory.ServerError, "Server error", isRetryable: true);
        var context = new RetryContext(_options);
        context.RecordAttempt(error); // First server error

        // Act
        var result = _sut.ShouldRetry(error, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CalculateDelay_RespectsRetryAfterHeader()
    {
        // Arrange
        var retryAfter = TimeSpan.FromSeconds(30);
        var error = new AgentError(
            Category: AgentErrorCategory.RateLimit,
            Message: "Rate limited",
            TechnicalDetails: null,
            IsRetryable: true,
            RetryAfter: retryAfter,
            ActionableGuidance: null,
            Exception: null,
            HttpStatusCode: 429,
            OccurredAt: DateTimeOffset.UtcNow);
        var context = new RetryContext(_options);

        // Act
        var delay = _sut.CalculateDelay(error, context);

        // Assert
        Assert.Equal(retryAfter, delay);
    }

    [Fact]
    public void CalculateDelay_UsesExponentialBackoff()
    {
        // Arrange
        var error = AgentError.Create(AgentErrorCategory.Timeout, "Timeout", isRetryable: true);
        var context = new RetryContext(_options);

        // Act
        var delay0 = _sut.CalculateDelay(error, context);
        context.RecordAttempt();
        var delay1 = _sut.CalculateDelay(error, context);
        context.RecordAttempt();
        var delay2 = _sut.CalculateDelay(error, context);

        // Assert (no jitter, so exact values)
        Assert.Equal(_options.InitialDelay, delay0);
        Assert.Equal(TimeSpan.FromMilliseconds(_options.InitialDelay.TotalMilliseconds * 2), delay1);
        Assert.Equal(TimeSpan.FromMilliseconds(_options.InitialDelay.TotalMilliseconds * 4), delay2);
    }

    [Fact]
    public void CalculateDelay_CapsAtMaxDelay()
    {
        // Arrange
        _options.InitialDelay = TimeSpan.FromMilliseconds(50);
        _options.MaxDelay = TimeSpan.FromMilliseconds(100);
        var error = AgentError.Create(AgentErrorCategory.Timeout, "Timeout", isRetryable: true);
        var context = new RetryContext(_options);
        context.RecordAttempt();
        context.RecordAttempt();
        context.RecordAttempt(); // Would be 50 * 2^3 = 400ms without cap

        // Act
        var delay = _sut.CalculateDelay(error, context);

        // Assert
        Assert.Equal(_options.MaxDelay, delay);
    }

    [Fact]
    public void RetryContext_TracksAttempts()
    {
        // Arrange
        var context = new RetryContext(_options);

        // Assert initial state
        Assert.Equal(0, context.AttemptCount);
        Assert.Equal(4, context.MaxAttempts); // 3 retries + 1 initial

        // Act
        context.RecordAttempt();
        context.RecordAttempt();

        // Assert
        Assert.Equal(2, context.AttemptCount);
    }

    [Fact]
    public void RetryContext_TracksServerErrors()
    {
        // Arrange
        var context = new RetryContext(_options);
        var serverError = AgentError.Create(AgentErrorCategory.ServerError, "Server error", isRetryable: true);
        var networkError = AgentError.Create(AgentErrorCategory.Network, "Network error", isRetryable: true);

        // Act
        context.RecordAttempt(serverError);
        context.RecordAttempt(networkError);
        context.RecordAttempt(serverError);

        // Assert
        Assert.Equal(3, context.AttemptCount);
        Assert.Equal(2, context.ServerErrorCount);
        Assert.Equal(3, context.Errors.Count);
    }

    [Fact]
    public void AgentOperationException_ContainsErrorAndAttemptCount()
    {
        // Arrange
        var error = AgentError.Create(AgentErrorCategory.Timeout, "Test error", isRetryable: true);

        // Act
        var exception = new AgentOperationException(error, 3);

        // Assert
        Assert.Equal(error, exception.Error);
        Assert.Equal(3, exception.AttemptCount);
        Assert.Equal("Test error", exception.Message);
    }
}
