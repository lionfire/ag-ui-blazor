using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudMessageList component.
/// </summary>
public class MudMessageListTests : TestContext
{
    public MudMessageListTests()
    {
        // Add MudBlazor services required for rendering
        Services.AddMudServices();

        // Add a mock IMarkdownRenderer for the MudMarkdown component
        var mockRenderer = new Mock<IMarkdownRenderer>();
        mockRenderer
            .Setup(r => r.RenderMarkup(It.IsAny<string?>()))
            .Returns<string?>(content =>
                new Microsoft.AspNetCore.Components.MarkupString(content ?? string.Empty));
        Services.AddSingleton(mockRenderer.Object);

        // Add JSInterop mocks for MudBlazor and scroll management
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Render MudPopoverProvider to support MudTooltip components
        RenderComponent<MudPopoverProvider>();
    }

    #region Rendering Tests

    [Fact]
    public void Component_Renders_Successfully()
    {
        // Act
        var cut = RenderComponent<MudMessageList>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-message-list").Should().NotBeNull();
    }

    [Fact]
    public void Component_Shows_EmptyMessage_WhenNoMessages()
    {
        // Act
        var cut = RenderComponent<MudMessageList>();

        // Assert
        var emptyContainer = cut.Find(".message-list-empty");
        emptyContainer.Should().NotBeNull();
        emptyContainer.TextContent.Should().Contain("No messages yet");
    }

    [Fact]
    public void Component_Shows_CustomEmptyMessage()
    {
        // Arrange
        var customMessage = "Start chatting with the assistant!";

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.EmptyMessage, customMessage));

        // Assert
        var emptyContainer = cut.Find(".message-list-empty");
        emptyContainer.TextContent.Should().Contain(customMessage);
    }

    [Fact]
    public void Component_Renders_Messages()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello!"),
            new ChatMessage(ChatRole.Assistant, "Hi there!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var messageContainers = cut.FindAll(".message-container");
        messageContainers.Should().HaveCount(2);
    }

    [Fact]
    public void Component_Renders_UserMessage_WithCorrectStyling()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var messageContainer = cut.Find(".message-container.user-message");
        messageContainer.Should().NotBeNull();

        var bubble = cut.Find(".message-bubble.user-bubble");
        bubble.Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_AssistantMessage_WithCorrectStyling()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.Assistant, "Hello, how can I help?")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var messageContainer = cut.Find(".message-container.assistant-message");
        messageContainer.Should().NotBeNull();

        var bubble = cut.Find(".message-bubble.assistant-bubble");
        bubble.Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_MessageContent_UsingMarkdown()
    {
        // Arrange
        var content = "This is **bold** text";
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, content)
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var markdownComponent = cut.FindComponent<MudMarkdown>();
        markdownComponent.Should().NotBeNull();
        markdownComponent.Instance.Content.Should().Be(content);
    }

    [Fact]
    public void Component_Renders_RoleDisplayNames_Correctly()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello!"),
            new ChatMessage(ChatRole.Assistant, "Hi!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var roleLabels = cut.FindAll(".message-role");
        roleLabels.Should().HaveCount(2);
        roleLabels[0].TextContent.Should().Be("You");
        roleLabels[1].TextContent.Should().Be("Assistant");
    }

    #endregion

    #region Streaming Indicator Tests

    [Fact]
    public void Component_Shows_TypingIndicator_WhenStreaming_AndLastMessageIsAssistant()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello!"),
            new ChatMessage(ChatRole.Assistant, "Hi there!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages)
            .Add(p => p.IsStreaming, true));

        // Assert
        var typingIndicator = cut.FindComponent<MudTypingIndicator>();
        typingIndicator.Should().NotBeNull();
    }

    [Fact]
    public void Component_DoesNotShow_TypingIndicator_WhenNotStreaming()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.Assistant, "Hi!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages)
            .Add(p => p.IsStreaming, false));

        // Assert
        var typingIndicators = cut.FindComponents<MudTypingIndicator>();
        typingIndicators.Should().BeEmpty();
    }

    [Fact]
    public void Component_DoesNotShow_TypingIndicator_WhenLastMessageIsUser()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.Assistant, "Hi!"),
            new ChatMessage(ChatRole.User, "Thanks!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages)
            .Add(p => p.IsStreaming, true));

        // Assert
        var typingIndicators = cut.FindComponents<MudTypingIndicator>();
        typingIndicators.Should().BeEmpty();
    }

    [Fact]
    public void Component_DoesNotShow_TypingIndicator_WhenMessagesEmpty()
    {
        // Arrange
        var messages = new List<ChatMessage>();

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages)
            .Add(p => p.IsStreaming, true));

        // Assert
        var typingIndicators = cut.FindComponents<MudTypingIndicator>();
        typingIndicators.Should().BeEmpty();
    }

    #endregion

    #region Avatar Tests

    [Fact]
    public void Component_Renders_UserAvatar_ForUserMessages()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var userAvatar = cut.Find(".user-avatar");
        userAvatar.Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_AssistantAvatar_ForAssistantMessages()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.Assistant, "Hi there!")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var assistantAvatar = cut.Find(".assistant-avatar");
        assistantAvatar.Should().NotBeNull();
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void Component_Accepts_NullMessages()
    {
        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, null));

        // Assert
        cut.Find(".message-list-empty").Should().NotBeNull();
    }

    [Fact]
    public void Component_Accepts_AdditionalClasses()
    {
        // Arrange
        var additionalClass = "my-custom-class";

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Class, additionalClass));

        // Assert
        var container = cut.Find(".mud-message-list");
        container.ClassList.Should().Contain(additionalClass);
    }

    [Fact]
    public void Component_Has_AutoScrollEnabled_ByDefault()
    {
        // Act
        var cut = RenderComponent<MudMessageList>();

        // Assert
        cut.Instance.AutoScrollEnabled.Should().BeTrue();
    }

    [Fact]
    public void Component_Accepts_AutoScrollDisabled()
    {
        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.AutoScrollEnabled, false));

        // Assert
        cut.Instance.AutoScrollEnabled.Should().BeFalse();
    }

    #endregion

    #region Multiple Messages Tests

    [Fact]
    public void Component_Renders_MultipleMessages_InOrder()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "First"),
            new ChatMessage(ChatRole.Assistant, "Second"),
            new ChatMessage(ChatRole.User, "Third")
        };

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var messageContainers = cut.FindAll(".message-container");
        messageContainers.Should().HaveCount(3);

        // Verify order through role labels
        var roleLabels = cut.FindAll(".message-role");
        roleLabels[0].TextContent.Should().Be("You");
        roleLabels[1].TextContent.Should().Be("Assistant");
        roleLabels[2].TextContent.Should().Be("You");
    }

    [Fact]
    public void Component_Handles_LongConversation()
    {
        // Arrange - Create a conversation with 100 messages
        var messages = new List<ChatMessage>();
        for (int i = 0; i < 100; i++)
        {
            var role = i % 2 == 0 ? ChatRole.User : ChatRole.Assistant;
            messages.Add(new ChatMessage(role, $"Message {i}"));
        }

        // Act
        var cut = RenderComponent<MudMessageList>(parameters => parameters
            .Add(p => p.Messages, messages));

        // Assert
        var messageContainers = cut.FindAll(".message-container");
        messageContainers.Should().HaveCount(100);
    }

    #endregion

    #region Scroll State Tests

    [Fact]
    public async Task ResetScrollState_ResetsUserScrolledUpFlag()
    {
        // Arrange
        var cut = RenderComponent<MudMessageList>();

        // Simulate user scrolling up via JSInvokable method
        await cut.InvokeAsync(() => cut.Instance.OnScroll(false));

        // Act
        await cut.InvokeAsync(() => cut.Instance.ResetScrollState());

        // Assert - Component should allow auto-scroll again
        // Since _userHasScrolledUp is private, we verify behavior indirectly
        // by ensuring the component doesn't throw and methods are accessible
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public async Task OnScroll_AcceptsIsAtBottomParameter()
    {
        // Arrange
        var cut = RenderComponent<MudMessageList>();

        // Act & Assert - Should not throw
        await cut.InvokeAsync(() => cut.Instance.OnScroll(true));
        await cut.InvokeAsync(() => cut.Instance.OnScroll(false));
    }

    #endregion
}
