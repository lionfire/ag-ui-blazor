using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Extensions;
using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;

namespace LionFire.AgUi.Blazor.Tests.Extensions;

/// <summary>
/// Tests for AgentClientFactoryExtensions.
/// </summary>
public class AgentClientFactoryExtensionsTests
{
    private readonly Mock<IAgentClientFactory> _mockFactory;
    private readonly Mock<IChatClient> _mockChatClient;

    public AgentClientFactoryExtensionsTests()
    {
        _mockFactory = new Mock<IAgentClientFactory>();
        _mockChatClient = new Mock<IChatClient>();
    }

    #region GetOrThrowAsync Tests

    [Fact]
    public async Task GetOrThrowAsync_WhenAgentExists_ShouldReturnAgent()
    {
        // Arrange
        _mockFactory
            .Setup(f => f.GetAgentAsync("test-agent", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockChatClient.Object);

        // Act
        var result = await _mockFactory.Object.GetOrThrowAsync("test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(_mockChatClient.Object);
    }

    [Fact]
    public async Task GetOrThrowAsync_WhenAgentNotFound_ShouldThrowWithHelpfulMessage()
    {
        // Arrange
        _mockFactory
            .Setup(f => f.GetAgentAsync("missing-agent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IChatClient?)null);

        _mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>
            {
                new AgentInfo("agent1", IsAvailable: true),
                new AgentInfo("agent2", IsAvailable: true),
                new AgentInfo("agent3", IsAvailable: false)
            });

        // Act
        var act = () => _mockFactory.Object.GetOrThrowAsync("missing-agent");

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("missing-agent");
        exception.Which.Message.Should().Contain("agent1");
        exception.Which.Message.Should().Contain("agent2");
        exception.Which.Message.Should().NotContain("agent3"); // Unavailable agent not listed
    }

    [Fact]
    public async Task GetOrThrowAsync_WhenNoAgentsAvailable_ShouldIndicateNoAgents()
    {
        // Arrange
        _mockFactory
            .Setup(f => f.GetAgentAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IChatClient?)null);

        _mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>());

        // Act
        var act = () => _mockFactory.Object.GetOrThrowAsync("missing");

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("No agents are currently available");
    }

    [Fact]
    public async Task GetOrThrowAsync_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        IAgentClientFactory? nullFactory = null;

        // Act
        var act = () => nullFactory!.GetOrThrowAsync("agent");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetOrThrowAsync_WithInvalidAgentName_ShouldThrowArgumentException(string? agentName)
    {
        // Act
        var act = () => _mockFactory.Object.GetOrThrowAsync(agentName!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetOrThrowAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockFactory
            .Setup(f => f.GetAgentAsync(It.IsAny<string>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _mockFactory.Object.GetOrThrowAsync("agent", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region IsAgentAvailableAsync Tests

    [Fact]
    public async Task IsAgentAvailableAsync_WhenAgentAvailable_ShouldReturnTrue()
    {
        // Arrange
        _mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>
            {
                new AgentInfo("available-agent", IsAvailable: true)
            });

        // Act
        var result = await _mockFactory.Object.IsAgentAvailableAsync("available-agent");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAgentAvailableAsync_WhenAgentNotAvailable_ShouldReturnFalse()
    {
        // Arrange
        _mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>
            {
                new AgentInfo("unavailable-agent", IsAvailable: false)
            });

        // Act
        var result = await _mockFactory.Object.IsAgentAvailableAsync("unavailable-agent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAgentAvailableAsync_WhenAgentNotFound_ShouldReturnFalse()
    {
        // Arrange
        _mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>
            {
                new AgentInfo("other-agent", IsAvailable: true)
            });

        // Act
        var result = await _mockFactory.Object.IsAgentAvailableAsync("missing-agent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAgentAvailableAsync_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        IAgentClientFactory? nullFactory = null;

        // Act
        var act = () => nullFactory!.IsAgentAvailableAsync("agent");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IsAgentAvailableAsync_WithInvalidAgentName_ShouldReturnFalse(string? agentName)
    {
        // Arrange
        _mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>());

        // Act
        var result = await _mockFactory.Object.IsAgentAvailableAsync(agentName!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAgentAvailableAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockFactory
            .Setup(f => f.ListAgentsAsync(cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _mockFactory.Object.IsAgentAvailableAsync("agent", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IsAgentAvailableAsync_WhenEmptyList_ShouldReturnFalse()
    {
        // Arrange
        _mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>());

        // Act
        var result = await _mockFactory.Object.IsAgentAvailableAsync("any-agent");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
