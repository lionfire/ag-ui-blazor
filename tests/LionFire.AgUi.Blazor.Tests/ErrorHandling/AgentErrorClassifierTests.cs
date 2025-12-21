using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using LionFire.AgUi.Blazor.ErrorHandling;

namespace LionFire.AgUi.Blazor.Tests.ErrorHandling;

public class AgentErrorClassifierTests
{
    [Fact]
    public void Classify_OperationCanceledException_ReturnsCancelledCategory()
    {
        // Arrange
        var exception = new OperationCanceledException();

        // Act
        var error = AgentErrorClassifier.Classify(exception);

        // Assert
        Assert.Equal(AgentErrorCategory.Cancelled, error.Category);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    public void Classify_TimeoutException_ReturnsTimeoutCategory()
    {
        // Arrange
        var exception = new TimeoutException("Request timed out");

        // Act
        var error = AgentErrorClassifier.Classify(exception);

        // Assert
        Assert.Equal(AgentErrorCategory.Timeout, error.Category);
        Assert.True(error.IsRetryable);
        Assert.NotNull(error.RetryAfter);
    }

    [Fact]
    public void Classify_SocketException_ReturnsNetworkCategory()
    {
        // Arrange
        var exception = new SocketException((int)SocketError.ConnectionRefused);

        // Act
        var error = AgentErrorClassifier.Classify(exception);

        // Assert
        Assert.Equal(AgentErrorCategory.Network, error.Category);
        Assert.True(error.IsRetryable);
    }

    [Fact]
    public void Classify_JsonException_ReturnsSerializationCategory()
    {
        // Arrange
        var exception = new System.Text.Json.JsonException("Invalid JSON");

        // Act
        var error = AgentErrorClassifier.Classify(exception);

        // Assert
        Assert.Equal(AgentErrorCategory.Serialization, error.Category);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    public void Classify_TaskCanceledException_WithCancellation_ReturnsCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var exception = new TaskCanceledException("Task was canceled", null, cts.Token);

        // Act
        var error = AgentErrorClassifier.Classify(exception);

        // Assert
        Assert.Equal(AgentErrorCategory.Cancelled, error.Category);
    }

    [Fact]
    public void Classify_TaskCanceledException_WithoutCancellation_ReturnsTimeout()
    {
        // Arrange
        var exception = new TaskCanceledException("Task timed out");

        // Act
        var error = AgentErrorClassifier.Classify(exception);

        // Assert
        Assert.Equal(AgentErrorCategory.Timeout, error.Category);
    }

    [Fact]
    public void Classify_UnknownException_ReturnsUnknownCategory()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var error = AgentErrorClassifier.Classify(exception);

        // Assert
        Assert.Equal(AgentErrorCategory.Unknown, error.Category);
        Assert.False(error.IsRetryable);
    }

    [Theory]
    [InlineData(400, AgentErrorCategory.BadRequest, false)]
    [InlineData(401, AgentErrorCategory.Authentication, false)]
    [InlineData(403, AgentErrorCategory.Authentication, false)]
    [InlineData(404, AgentErrorCategory.ModelNotFound, false)]
    [InlineData(429, AgentErrorCategory.RateLimit, true)]
    [InlineData(500, AgentErrorCategory.ServerError, true)]
    [InlineData(502, AgentErrorCategory.ServerError, true)]
    [InlineData(503, AgentErrorCategory.ServiceUnavailable, true)]
    [InlineData(504, AgentErrorCategory.Timeout, true)]
    public void ClassifyHttpStatus_ReturnsCorrectCategory(int statusCode, AgentErrorCategory expectedCategory, bool expectedRetryable)
    {
        // Act
        var error = AgentErrorClassifier.ClassifyHttpStatus(statusCode);

        // Assert
        Assert.Equal(expectedCategory, error.Category);
        Assert.Equal(expectedRetryable, error.IsRetryable);
        Assert.Equal(statusCode, error.HttpStatusCode);
    }

    [Fact]
    public void ClassifyHttpStatus_400WithContextLength_ReturnsContextLengthExceeded()
    {
        // Act
        var error = AgentErrorClassifier.ClassifyHttpStatus(400, "context_length exceeded: token limit reached");

        // Assert
        Assert.Equal(AgentErrorCategory.ContextLengthExceeded, error.Category);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    public void ClassifyHttpStatus_400WithContentPolicy_ReturnsContentPolicy()
    {
        // Act
        var error = AgentErrorClassifier.ClassifyHttpStatus(400, "content_policy violation detected");

        // Assert
        Assert.Equal(AgentErrorCategory.ContentPolicy, error.Category);
        Assert.False(error.IsRetryable);
    }

    [Theory]
    [InlineData("60", 60)]
    [InlineData("120", 120)]
    [InlineData("5", 5)]
    public void ParseRetryAfter_WithSeconds_ReturnsCorrectDuration(string header, int expectedSeconds)
    {
        // Act
        var result = AgentErrorClassifier.ParseRetryAfter(header);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result);
    }

    [Fact]
    public void ParseRetryAfter_WithNull_ReturnsNull()
    {
        // Act
        var result = AgentErrorClassifier.ParseRetryAfter(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseRetryAfter_WithEmptyString_ReturnsNull()
    {
        // Act
        var result = AgentErrorClassifier.ParseRetryAfter("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ClassifyHttpStatus_429WithRetryInfo_ExtractsRetryAfter()
    {
        // Act
        var error = AgentErrorClassifier.ClassifyHttpStatus(429, "Rate limit exceeded. Please retry after 30 seconds.");

        // Assert
        Assert.Equal(AgentErrorCategory.RateLimit, error.Category);
        Assert.True(error.IsRetryable);
        Assert.NotNull(error.RetryAfter);
        Assert.Equal(TimeSpan.FromSeconds(30), error.RetryAfter);
    }

    [Fact]
    public void Classify_ThrowsOnNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AgentErrorClassifier.Classify(null!));
    }
}
