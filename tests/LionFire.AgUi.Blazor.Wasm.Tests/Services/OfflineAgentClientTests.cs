using FluentAssertions;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Wasm.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;

namespace LionFire.AgUi.Blazor.Wasm.Tests.Services;

public class OfflineAgentClientTests
{
    private readonly Mock<IChatClient> _mockInnerClient;
    private readonly Mock<IConnectionMonitor> _mockConnectionMonitor;
    private readonly Mock<IOfflineMessageQueue> _mockMessageQueue;
    private readonly Mock<ILogger> _mockLogger;
    private readonly OfflineAgentClient _client;
    private const string AgentName = "test-agent";

    public OfflineAgentClientTests()
    {
        _mockInnerClient = new Mock<IChatClient>();
        _mockConnectionMonitor = new Mock<IConnectionMonitor>();
        _mockMessageQueue = new Mock<IOfflineMessageQueue>();
        _mockLogger = new Mock<ILogger>();

        _mockInnerClient.Setup(x => x.Metadata)
            .Returns(new ChatClientMetadata("test", new Uri("http://localhost")));

        _client = new OfflineAgentClient(
            _mockInnerClient.Object,
            _mockConnectionMonitor.Object,
            _mockMessageQueue.Object,
            AgentName,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenInnerClientIsNull()
    {
        Action act = () => new OfflineAgentClient(
            null!,
            _mockConnectionMonitor.Object,
            _mockMessageQueue.Object,
            AgentName,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("innerClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConnectionMonitorIsNull()
    {
        Action act = () => new OfflineAgentClient(
            _mockInnerClient.Object,
            null!,
            _mockMessageQueue.Object,
            AgentName,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("connectionMonitor");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMessageQueueIsNull()
    {
        Action act = () => new OfflineAgentClient(
            _mockInnerClient.Object,
            _mockConnectionMonitor.Object,
            null!,
            AgentName,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("messageQueue");
    }

    [Fact]
    public void Metadata_ReturnsInnerClientMetadata()
    {
        var metadata = _client.Metadata;

        metadata.Should().NotBeNull();
        metadata.ProviderName.Should().Be("test");
    }

    [Fact]
    public async Task CompleteAsync_WhenOnline_CallsInnerClient()
    {
        // Arrange
        _mockConnectionMonitor.Setup(x => x.IsOnline).Returns(true);
        _mockMessageQueue.Setup(x => x.PeekAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueuedMessage?)null);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hello") };
        var expectedCompletion = new ChatCompletion(new ChatMessage(ChatRole.Assistant, "Hi"));

        _mockInnerClient.Setup(x => x.CompleteAsync(messages, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCompletion);

        // Act
        var result = await _client.CompleteAsync(messages);

        // Assert
        result.Should().Be(expectedCompletion);
        _mockInnerClient.Verify(x => x.CompleteAsync(messages, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenOffline_QueuesMessageAndThrowsOfflineException()
    {
        // Arrange
        _mockConnectionMonitor.Setup(x => x.IsOnline).Returns(false);
        var messages = new List<ChatMessage> { new(ChatRole.User, "Hello") };

        // Act & Assert
        var act = async () => await _client.CompleteAsync(messages);
        await act.Should().ThrowAsync<OfflineException>();

        _mockMessageQueue.Verify(x => x.EnqueueAsync(
            It.Is<QueuedMessage>(m =>
                m.AgentName == AgentName &&
                m.Messages.Count == 1),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenOnlineButHttpFails_QueuesMessageAndThrows()
    {
        // Arrange
        _mockConnectionMonitor.Setup(x => x.IsOnline).Returns(true);
        _mockMessageQueue.Setup(x => x.PeekAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueuedMessage?)null);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hello") };

        _mockInnerClient.Setup(x => x.CompleteAsync(messages, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var act = async () => await _client.CompleteAsync(messages);
        await act.Should().ThrowAsync<OfflineException>();

        _mockMessageQueue.Verify(x => x.EnqueueAsync(
            It.IsAny<QueuedMessage>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenOnline_ProcessesQueuedMessagesFirst()
    {
        // Arrange
        _mockConnectionMonitor.Setup(x => x.IsOnline).Returns(true);

        var queuedMessage = new QueuedMessage
        {
            Id = "queued-1",
            AgentName = AgentName,
            Messages = new List<ChatMessage> { new(ChatRole.User, "Queued message") }
        };

        var callCount = 0;
        _mockMessageQueue.Setup(x => x.PeekAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? queuedMessage : null;
            });

        var newMessages = new List<ChatMessage> { new(ChatRole.User, "New message") };
        var expectedCompletion = new ChatCompletion(new ChatMessage(ChatRole.Assistant, "Response"));

        _mockInnerClient.Setup(x => x.CompleteAsync(It.IsAny<IList<ChatMessage>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCompletion);

        // Act
        await _client.CompleteAsync(newMessages);

        // Assert
        _mockMessageQueue.Verify(x => x.MarkDeliveredAsync(queuedMessage.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void GetService_ReturnsOfflineAgentClient_WhenTypeMatches()
    {
        var result = _client.GetService<OfflineAgentClient>();

        result.Should().Be(_client);
    }

    [Fact]
    public void GetServiceByType_ReturnsOfflineAgentClient_WhenTypeMatches()
    {
        var result = _client.GetService(typeof(OfflineAgentClient));

        result.Should().Be(_client);
    }

    [Fact]
    public void GetService_ReturnsNull_WhenTypeDoesNotMatch()
    {
        // GetService<T> is an extension method, so we test through GetService(Type, object?)
        var result = _client.GetService(typeof(string), null);

        // When the type doesn't match OfflineAgentClient, it delegates to inner client
        // The mock returns null by default
        result.Should().BeNull();
    }

    [Fact]
    public void Dispose_DisposesInnerClient()
    {
        _client.Dispose();

        _mockInnerClient.Verify(x => x.Dispose(), Times.Once);
    }
}

public class QueuedMessageTests
{
    [Fact]
    public void NewQueuedMessage_HasUniqueId()
    {
        var msg1 = new QueuedMessage();
        var msg2 = new QueuedMessage();

        msg1.Id.Should().NotBeNullOrEmpty();
        msg2.Id.Should().NotBeNullOrEmpty();
        msg1.Id.Should().NotBe(msg2.Id);
    }

    [Fact]
    public void NewQueuedMessage_HasCorrectDefaults()
    {
        var msg = new QueuedMessage();

        msg.AgentName.Should().BeEmpty();
        msg.Messages.Should().BeEmpty();
        msg.AttemptCount.Should().Be(0);
        msg.MaxAttempts.Should().Be(5);
        msg.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenAttemptCountEqualsMaxAttempts()
    {
        var msg = new QueuedMessage
        {
            AttemptCount = 5,
            MaxAttempts = 5
        };

        msg.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenAttemptCountLessThanMaxAttempts()
    {
        var msg = new QueuedMessage
        {
            AttemptCount = 4,
            MaxAttempts = 5
        };

        msg.IsExpired.Should().BeFalse();
    }
}

public class OfflineExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new OfflineException("Test message");

        ex.Message.Should().Be("Test message");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new Exception("Inner");
        var ex = new OfflineException("Test message", inner);

        ex.Message.Should().Be("Test message");
        ex.InnerException.Should().Be(inner);
    }
}
