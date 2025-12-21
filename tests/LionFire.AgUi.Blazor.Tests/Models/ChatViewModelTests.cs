using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for the ChatViewModel class.
/// </summary>
public class ChatViewModelTests
{
    [Fact]
    public void DefaultValues_ShouldBeInitialized()
    {
        var viewModel = new ChatViewModel();

        viewModel.ConversationId.Should().BeEmpty();
        viewModel.Messages.Should().NotBeNull();
        viewModel.Messages.Should().BeEmpty();
        viewModel.IsStreaming.Should().BeFalse();
        viewModel.ConnectionState.Should().Be(ConnectionState.Disconnected);
        viewModel.PendingToolCalls.Should().NotBeNull();
        viewModel.PendingToolCalls.Should().BeEmpty();
        viewModel.ErrorMessage.Should().BeNull();
        viewModel.TokenUsage.Should().BeNull();
    }

    [Fact]
    public void HasPendingToolCalls_ShouldReturnTrue_WhenToolCallsExist()
    {
        var viewModel = new ChatViewModel();
        viewModel.PendingToolCalls.Add(ToolCall.Create("test_tool"));

        viewModel.HasPendingToolCalls.Should().BeTrue();
    }

    [Fact]
    public void HasPendingToolCalls_ShouldReturnFalse_WhenNoToolCalls()
    {
        var viewModel = new ChatViewModel();

        viewModel.HasPendingToolCalls.Should().BeFalse();
    }

    [Fact]
    public void HasError_ShouldReturnTrue_WhenErrorMessageSet()
    {
        var viewModel = new ChatViewModel
        {
            ErrorMessage = "Something went wrong"
        };

        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public void HasError_ShouldReturnFalse_WhenNoError()
    {
        var viewModel = new ChatViewModel();

        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public void HasError_ShouldReturnFalse_WhenErrorMessageIsEmpty()
    {
        var viewModel = new ChatViewModel
        {
            ErrorMessage = ""
        };

        viewModel.HasError.Should().BeFalse();
    }

    [Theory]
    [InlineData(ConnectionState.Connected, false, false, true)]
    [InlineData(ConnectionState.Connecting, false, false, false)]
    [InlineData(ConnectionState.Connected, true, false, false)]
    [InlineData(ConnectionState.Connected, false, true, false)]
    public void IsReady_ShouldReturnCorrectValue(
        ConnectionState connectionState,
        bool isStreaming,
        bool hasPendingCalls,
        bool expectedResult)
    {
        var viewModel = new ChatViewModel
        {
            ConnectionState = connectionState,
            IsStreaming = isStreaming
        };

        if (hasPendingCalls)
        {
            viewModel.PendingToolCalls.Add(ToolCall.Create("tool"));
        }

        viewModel.IsReady.Should().Be(expectedResult);
    }

    [Fact]
    public void ClearError_ShouldSetErrorMessageToNull()
    {
        var viewModel = new ChatViewModel
        {
            ErrorMessage = "Error occurred"
        };

        viewModel.ClearError();

        viewModel.ErrorMessage.Should().BeNull();
        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public void Reset_ShouldClearAllState()
    {
        var viewModel = new ChatViewModel
        {
            ConversationId = "conv-123",
            IsStreaming = true,
            ConnectionState = ConnectionState.Connected,
            ErrorMessage = "Error",
            TokenUsage = TokenUsage.Create(100, 50)
        };
        viewModel.Messages.Add(new ChatMessage(ChatRole.User, "Hello"));
        viewModel.PendingToolCalls.Add(ToolCall.Create("tool"));

        viewModel.Reset();

        viewModel.ConversationId.Should().BeEmpty();
        viewModel.Messages.Should().BeEmpty();
        viewModel.IsStreaming.Should().BeFalse();
        viewModel.ConnectionState.Should().Be(ConnectionState.Disconnected);
        viewModel.PendingToolCalls.Should().BeEmpty();
        viewModel.ErrorMessage.Should().BeNull();
        viewModel.TokenUsage.Should().BeNull();
    }

    [Fact]
    public void ToConversation_ShouldCreateImmutableSnapshot()
    {
        var viewModel = new ChatViewModel
        {
            ConversationId = "conv-123"
        };
        viewModel.Messages.Add(new ChatMessage(ChatRole.User, "Hello"));
        viewModel.Messages.Add(new ChatMessage(ChatRole.Assistant, "Hi!"));

        var conversation = viewModel.ToConversation("test-agent");

        conversation.Id.Should().Be("conv-123");
        conversation.AgentName.Should().Be("test-agent");
        conversation.Messages.Should().HaveCount(2);
        conversation.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        conversation.LastModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToConversation_ShouldCreateIndependentCopy()
    {
        var viewModel = new ChatViewModel
        {
            ConversationId = "conv-123"
        };
        viewModel.Messages.Add(new ChatMessage(ChatRole.User, "Hello"));

        var conversation = viewModel.ToConversation("agent");

        // Modify the view model
        viewModel.Messages.Add(new ChatMessage(ChatRole.Assistant, "Hi!"));

        // Conversation should be unchanged
        conversation.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void Messages_ShouldBeMutable()
    {
        var viewModel = new ChatViewModel();

        viewModel.Messages.Add(new ChatMessage(ChatRole.User, "First"));
        viewModel.Messages.Add(new ChatMessage(ChatRole.Assistant, "Second"));

        viewModel.Messages.Should().HaveCount(2);

        viewModel.Messages.Clear();
        viewModel.Messages.Should().BeEmpty();
    }
}
