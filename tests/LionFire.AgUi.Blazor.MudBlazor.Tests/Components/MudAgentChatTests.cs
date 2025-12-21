using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudAgentChat component.
/// </summary>
public partial class MudAgentChatTests : TestContext
{
    private readonly Mock<IAgentClientFactory> _mockAgentFactory;
    private readonly Mock<IChatClient> _mockChatClient;
    private readonly Mock<ILogger<MudAgentChat>> _mockLogger;

    public MudAgentChatTests()
    {
        // Add MudBlazor services required for rendering
        Services.AddMudServices();

        // Add JSInterop mocks for MudBlazor and scroll management
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Setup mocks
        _mockAgentFactory = new Mock<IAgentClientFactory>();
        _mockChatClient = new Mock<IChatClient>();
        _mockLogger = new Mock<ILogger<MudAgentChat>>();

        // Add a mock IMarkdownRenderer for the MudMarkdown component
        var mockRenderer = new Mock<IMarkdownRenderer>();
        mockRenderer
            .Setup(r => r.RenderMarkup(It.IsAny<string?>()))
            .Returns<string?>(content =>
                new Microsoft.AspNetCore.Components.MarkupString(content ?? string.Empty));

        // Register services
        Services.AddSingleton(_mockAgentFactory.Object);
        Services.AddSingleton(_mockLogger.Object);
        Services.AddSingleton(mockRenderer.Object);

        // Add keyboard shortcut service
        var keyboardService = new KeyboardShortcutService();
        keyboardService.RegisterDefaults();
        Services.AddSingleton<IKeyboardShortcutService>(keyboardService);
    }

    private void SetupAgentFactory(IChatClient? agent = null, ConnectionState state = ConnectionState.Connected)
    {
        _mockAgentFactory
            .Setup(f => f.GetAgentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(agent);

        _mockAgentFactory
            .Setup(f => f.GetConnectionState())
            .Returns(state);
    }

    #region Rendering Tests

    [Fact]
    public void Component_Renders_Successfully()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-agent-chat").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_WithMudPaper()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        var paper = cut.FindComponent<MudPaper>();
        paper.Should().NotBeNull();
        paper.Instance.Elevation.Should().Be(4);
    }

    [Fact]
    public void Component_Renders_MessageList()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        var messageList = cut.FindComponent<MudMessageList>();
        messageList.Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_MessageInput()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        var messageInput = cut.FindComponent<MudMessageInput>();
        messageInput.Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_ConnectionStatus_ByDefault()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        var connectionStatus = cut.FindComponent<MudConnectionStatus>();
        connectionStatus.Should().NotBeNull();
    }

    [Fact]
    public void Component_Hides_ConnectionStatus_WhenDisabled()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent")
            .Add(p => p.ShowConnectionStatus, false));

        // Assert
        var connectionStatuses = cut.FindComponents<MudConnectionStatus>();
        connectionStatuses.Should().BeEmpty();
    }

    #endregion

    #region Agent Initialization Tests

    [Fact]
    public async Task Component_Initializes_Agent_OnInitialized()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert - Wait for async initialization
        await Task.Delay(50);
        _mockAgentFactory.Verify(f => f.GetAgentAsync("test-agent", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Component_Shows_Error_WhenAgentNotFound()
    {
        // Arrange
        SetupAgentFactory(null, ConnectionState.Error);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "nonexistent-agent"));

        // Assert
        cut.Instance.ErrorMessage.Should().Contain("not found");
        cut.Instance.ConnectionState.Should().Be(ConnectionState.Error);
    }

    [Fact]
    public void Component_Throws_WhenAgentNameIsEmpty()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act & Assert
        var action = () => RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, string.Empty));

        action.Should().Throw<ArgumentException>()
            .WithMessage("*AgentName is required*");
    }

    [Fact]
    public void Component_Throws_WhenAgentNameIsWhitespace()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act & Assert
        var action = () => RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "   "));

        action.Should().Throw<ArgumentException>()
            .WithMessage("*AgentName is required*");
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void Component_Accepts_CssClass()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);
        var customClass = "my-custom-chat";

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent")
            .Add(p => p.CssClass, customClass));

        // Assert
        var container = cut.Find(".mud-agent-chat");
        container.ClassList.Should().Contain(customClass);
    }

    [Fact]
    public void Component_ShowConnectionStatus_DefaultsToTrue()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        cut.Instance.Should().NotBeNull();
        var connectionStatuses = cut.FindComponents<MudConnectionStatus>();
        connectionStatuses.Should().NotBeEmpty();
    }

    #endregion

    #region State Tests

    [Fact]
    public void Component_Messages_IsEmpty_Initially()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        cut.Instance.Messages.Should().BeEmpty();
    }

    [Fact]
    public void Component_IsStreaming_IsFalse_Initially()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        cut.Instance.IsStreaming.Should().BeFalse();
    }

    [Fact]
    public void Component_ConnectionState_IsConnected_WhenAgentAvailable()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object, ConnectionState.Connected);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        cut.Instance.ConnectionState.Should().Be(ConnectionState.Connected);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Component_Shows_Error_Alert_WhenErrorOccurs()
    {
        // Arrange
        _mockAgentFactory
            .Setup(f => f.GetAgentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        _mockAgentFactory
            .Setup(f => f.GetConnectionState())
            .Returns(ConnectionState.Error);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        cut.Instance.ErrorMessage.Should().Contain("Connection failed");

        // Alert should be visible when there's an error
        var alerts = cut.FindComponents<MudAlert>();
        alerts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ClearError_ClearsErrorMessage()
    {
        // Arrange
        SetupAgentFactory(null, ConnectionState.Error);

        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Verify error exists
        cut.Instance.ErrorMessage.Should().NotBeNull();

        // Act
        await cut.InvokeAsync(() => cut.Instance.ClearError());

        // Assert
        cut.Instance.ErrorMessage.Should().BeNull();
    }

    #endregion

    #region Message Sending Tests

    [Fact]
    public async Task SendMessageAsync_DoesNothing_WhenMessageIsEmpty()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessageAsync(string.Empty));

        // Assert
        cut.Instance.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SendMessageAsync_DoesNothing_WhenMessageIsWhitespace()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessageAsync("   "));

        // Assert
        cut.Instance.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SendMessageAsync_SetsError_WhenAgentIsNull()
    {
        // Arrange
        SetupAgentFactory(null, ConnectionState.Error);
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Clear the initialization error first
        await cut.InvokeAsync(() => cut.Instance.ClearError());

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessageAsync("Hello"));

        // Assert
        cut.Instance.ErrorMessage.Should().Contain("not available");
    }

    #endregion

    #region ClearConversation Tests

    [Fact]
    public async Task ClearConversation_ClearsAllMessages()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.ClearConversation());

        // Assert
        cut.Instance.Messages.Should().BeEmpty();
    }

    #endregion

    #region Callback Tests

    [Fact]
    public async Task OnMessageSent_IsInvoked_WhenMessageSent()
    {
        // Arrange
        ChatMessage? sentMessage = null;
        SetupAgentFactory(_mockChatClient.Object);

        // Setup streaming response
        var streamingResponses = new List<StreamingChatCompletionUpdate>
        {
            new StreamingChatCompletionUpdate { Text = "Hello" }
        };

        _mockChatClient
            .Setup(c => c.CompleteStreamingAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
            .Returns(streamingResponses.ToAsyncEnumerable());

        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent")
            .Add(p => p.OnMessageSent, (ChatMessage msg) => sentMessage = msg));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessageAsync("Test message"));

        // Assert
        sentMessage.Should().NotBeNull();
        sentMessage!.Text.Should().Be("Test message");
    }

    [Fact]
    public async Task OnMessageReceived_IsInvoked_WhenResponseReceived()
    {
        // Arrange
        ChatMessage? receivedMessage = null;
        SetupAgentFactory(_mockChatClient.Object);

        // Setup streaming response
        var streamingResponses = new List<StreamingChatCompletionUpdate>
        {
            new StreamingChatCompletionUpdate { Text = "Response text" }
        };

        _mockChatClient
            .Setup(c => c.CompleteStreamingAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
            .Returns(streamingResponses.ToAsyncEnumerable());

        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent")
            .Add(p => p.OnMessageReceived, (ChatMessage msg) => receivedMessage = msg));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessageAsync("Test message"));

        // Assert
        receivedMessage.Should().NotBeNull();
        receivedMessage!.Role.Should().Be(ChatRole.Assistant);
    }

    #endregion

    #region MessageInput Disabled State Tests

    [Fact]
    public void MessageInput_IsDisabled_WhenAgentIsNull()
    {
        // Arrange
        SetupAgentFactory(null, ConnectionState.Error);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        var messageInput = cut.FindComponent<MudMessageInput>();
        messageInput.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public void MessageInput_IsDisabled_WhenConnectionError()
    {
        // Arrange
        _mockAgentFactory
            .Setup(f => f.GetAgentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection error"));

        _mockAgentFactory
            .Setup(f => f.GetConnectionState())
            .Returns(ConnectionState.Error);

        // Act
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Assert
        var messageInput = cut.FindComponent<MudMessageInput>();
        messageInput.Instance.Disabled.Should().BeTrue();
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Component_Implements_IDisposable()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Act & Assert - Should not throw
        cut.Instance.Dispose();
    }

    #endregion

    #region Message Operations Tests

    [Fact]
    public void CancelStreaming_DoesNotThrow_WhenNotStreaming()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);
        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Act & Assert - Should not throw
        cut.InvokeAsync(() => cut.Instance.CancelStreaming());
    }

    [Fact]
    public async Task CancelStreaming_StopsStreaming_WhenStreaming()
    {
        // Arrange
        SetupAgentFactory(_mockChatClient.Object);

        // Setup a streaming response that will take a long time
        var tcs = new TaskCompletionSource<bool>();
        var streamingResponses = CreateSlowStreamingResponses(tcs);

        _mockChatClient
            .Setup(c => c.CompleteStreamingAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
            .Returns(streamingResponses);

        var cut = RenderComponent<MudAgentChat>(parameters => parameters
            .Add(p => p.AgentName, "test-agent"));

        // Start streaming (don't await)
        var sendTask = cut.InvokeAsync(() => cut.Instance.SendMessageAsync("Hello"));

        // Give it a moment to start streaming
        await Task.Delay(50);

        // Act - Cancel while streaming
        await cut.InvokeAsync(() => cut.Instance.CancelStreaming());

        // Complete the slow stream to allow the task to finish
        tcs.SetResult(true);
        await sendTask;

        // Assert - Should not be streaming anymore
        cut.Instance.IsStreaming.Should().BeFalse();
    }

    #endregion
}

/// <summary>
/// Extension methods for creating async enumerables in tests.
/// </summary>
internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}

/// <summary>
/// Partial class for helper methods.
/// </summary>
public partial class MudAgentChatTests
{
    private static async IAsyncEnumerable<StreamingChatCompletionUpdate> CreateSlowStreamingResponses(
        TaskCompletionSource<bool> tcs,
        [System.Runtime.CompilerServices.EnumeratorCancellation] System.Threading.CancellationToken cancellationToken = default)
    {
        yield return new StreamingChatCompletionUpdate { Text = "Hello" };

        // Wait for cancellation or completion
        try
        {
            await Task.WhenAny(tcs.Task, Task.Delay(5000, cancellationToken));
        }
        catch (OperationCanceledException)
        {
            yield break;
        }

        yield return new StreamingChatCompletionUpdate { Text = " world" };
    }
}
