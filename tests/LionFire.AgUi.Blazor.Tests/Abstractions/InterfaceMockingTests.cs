using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;

namespace LionFire.AgUi.Blazor.Tests.Abstractions;

/// <summary>
/// Tests that interfaces can be properly mocked for unit testing.
/// </summary>
public class InterfaceMockingTests
{
    [Fact]
    public async Task IAgentClientFactory_CanBeMocked()
    {
        // Arrange
        var mockFactory = new Mock<IAgentClientFactory>();
        var mockChatClient = new Mock<IChatClient>();

        mockFactory
            .Setup(f => f.GetAgentAsync("test-agent", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockChatClient.Object);

        mockFactory
            .Setup(f => f.ListAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentInfo>
            {
                new AgentInfo("test-agent", "Test agent", null, true),
                new AgentInfo("other-agent", "Other agent", null, false)
            });

        mockFactory
            .Setup(f => f.GetConnectionState())
            .Returns(ConnectionState.Connected);

        // Act
        var agent = await mockFactory.Object.GetAgentAsync("test-agent");
        var agents = await mockFactory.Object.ListAgentsAsync();
        var state = mockFactory.Object.GetConnectionState();

        // Assert
        agent.Should().NotBeNull();
        agents.Should().HaveCount(2);
        state.Should().Be(ConnectionState.Connected);
    }

    [Fact]
    public async Task IAgentClientFactory_CanReturnNull()
    {
        // Arrange
        var mockFactory = new Mock<IAgentClientFactory>();
        mockFactory
            .Setup(f => f.GetAgentAsync("unknown-agent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IChatClient?)null);

        // Act
        var agent = await mockFactory.Object.GetAgentAsync("unknown-agent");

        // Assert
        agent.Should().BeNull();
    }

    [Fact]
    public async Task IAgentStateManager_CanBeMocked()
    {
        // Arrange
        var mockManager = new Mock<IAgentStateManager>();
        var conversation = Conversation.Create("agent");

        mockManager
            .Setup(m => m.SaveConversationAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockManager
            .Setup(m => m.LoadConversationAsync(conversation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        mockManager
            .Setup(m => m.ListConversationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConversationMetadata>
            {
                ConversationMetadata.FromConversation(conversation)
            });

        mockManager
            .Setup(m => m.DeleteConversationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockManager.Object.SaveConversationAsync(conversation);
        var loaded = await mockManager.Object.LoadConversationAsync(conversation.Id);
        var list = await mockManager.Object.ListConversationsAsync();
        await mockManager.Object.DeleteConversationAsync(conversation.Id);

        // Assert
        loaded.Should().Be(conversation);
        list.Should().HaveCount(1);

        mockManager.Verify(m => m.SaveConversationAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Once);
        mockManager.Verify(m => m.DeleteConversationAsync(conversation.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IAgentStateManager_LoadCanReturnNull()
    {
        // Arrange
        var mockManager = new Mock<IAgentStateManager>();
        mockManager
            .Setup(m => m.LoadConversationAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        var result = await mockManager.Object.LoadConversationAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IToolApprovalService_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<IToolApprovalService>();
        var toolCall = ToolCall.Create("read_file");

        mockService
            .Setup(s => s.RequestApprovalAsync(It.IsAny<ToolCall>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ToolApprovalResult.Approved());

        mockService
            .Setup(s => s.ShouldApproveAutomaticallyAsync(It.IsAny<ToolCall>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mockService
            .SetupGet(s => s.ApprovalMode)
            .Returns(ToolApprovalMode.Blocking);

        // Act
        var result = await mockService.Object.RequestApprovalAsync(toolCall);
        var shouldAuto = await mockService.Object.ShouldApproveAutomaticallyAsync(toolCall);
        var mode = mockService.Object.ApprovalMode;

        // Assert
        result.IsApproved.Should().BeTrue();
        shouldAuto.Should().BeTrue();
        mode.Should().Be(ToolApprovalMode.Blocking);
    }

    [Fact]
    public async Task IToolApprovalService_CanDenyToolCalls()
    {
        // Arrange
        var mockService = new Mock<IToolApprovalService>();
        var dangerousTool = ToolCall.Create("delete_all", riskLevel: ToolRiskLevel.Dangerous);

        mockService
            .Setup(s => s.RequestApprovalAsync(
                It.Is<ToolCall>(t => t.RiskLevel == ToolRiskLevel.Dangerous),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ToolApprovalResult.Denied("Dangerous operations require explicit approval"));

        mockService
            .Setup(s => s.ShouldApproveAutomaticallyAsync(
                It.Is<ToolCall>(t => t.RiskLevel == ToolRiskLevel.Dangerous),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await mockService.Object.RequestApprovalAsync(dangerousTool);
        var shouldAuto = await mockService.Object.ShouldApproveAutomaticallyAsync(dangerousTool);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.DenialReason.Should().Contain("Dangerous");
        shouldAuto.Should().BeFalse();
    }

    [Fact]
    public async Task AllInterfaces_SupportCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var mockFactory = new Mock<IAgentClientFactory>();
        var mockManager = new Mock<IAgentStateManager>();
        var mockApproval = new Mock<IToolApprovalService>();

        mockFactory
            .Setup(f => f.GetAgentAsync(It.IsAny<string>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        mockManager
            .Setup(m => m.LoadConversationAsync(It.IsAny<string>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        mockApproval
            .Setup(s => s.RequestApprovalAsync(It.IsAny<ToolCall>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mockFactory.Object.GetAgentAsync("agent", cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mockManager.Object.LoadConversationAsync("id", cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mockApproval.Object.RequestApprovalAsync(ToolCall.Create("tool"), cts.Token));
    }
}
