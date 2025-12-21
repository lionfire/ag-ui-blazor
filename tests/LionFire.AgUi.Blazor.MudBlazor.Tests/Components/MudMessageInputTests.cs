using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudMessageInput component.
/// </summary>
public class MudMessageInputTests : TestContext
{
    public MudMessageInputTests()
    {
        // Add MudBlazor services required for rendering
        Services.AddMudServices();

        // Add JSInterop mocks for MudBlazor
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Component_Renders_Successfully()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-message-input").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_WithDefaultPlaceholder()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>();

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.Placeholder.Should().Be("Type a message...");
    }

    [Fact]
    public void Component_Renders_WithCustomPlaceholder()
    {
        // Arrange
        var placeholder = "Enter your message here...";

        // Act
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.Placeholder, placeholder));

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.Placeholder.Should().Be(placeholder);
    }

    [Fact]
    public void Component_Renders_SendButton()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>();

        // Assert
        var iconButton = cut.FindComponent<MudIconButton>();
        iconButton.Should().NotBeNull();
    }

    [Fact]
    public void SendButton_IsDisabled_WhenMessageIsEmpty()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>();

        // Assert
        var iconButton = cut.FindComponent<MudIconButton>();
        iconButton.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public void SendButton_IsDisabled_WhenComponentIsDisabled()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        var iconButton = cut.FindComponent<MudIconButton>();
        iconButton.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public void TextField_IsDisabled_WhenComponentIsDisabled()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public async Task OnSend_IsCalled_WhenSendMessageIsInvoked()
    {
        // Arrange
        var sentMessage = string.Empty;
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string msg) => sentMessage = msg));

        // Use InvokeAsync to set message within renderer context
        await cut.InvokeAsync(() => cut.Instance.SetMessage("Hello, world!"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        // Assert
        sentMessage.Should().Be("Hello, world!");
    }

    [Fact]
    public async Task SendMessage_ClearsInput_AfterSending()
    {
        // Arrange
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string _) => { }));

        await cut.InvokeAsync(() => cut.Instance.SetMessage("Test message"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        // Assert
        cut.Instance.Message.Should().BeEmpty();
    }

    [Fact]
    public async Task SendMessage_DoesNothing_WhenDisabled()
    {
        // Arrange
        var wasCalled = false;
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.OnSend, (string _) => wasCalled = true));

        await cut.InvokeAsync(() => cut.Instance.SetMessage("Test message"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        // Assert
        wasCalled.Should().BeFalse();
        cut.Instance.Message.Should().Be("Test message");
    }

    [Fact]
    public async Task SendMessage_DoesNothing_WhenMessageIsEmpty()
    {
        // Arrange
        var wasCalled = false;
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string _) => wasCalled = true));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        // Assert
        wasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessage_DoesNothing_WhenMessageIsWhitespace()
    {
        // Arrange
        var wasCalled = false;
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string _) => wasCalled = true));

        await cut.InvokeAsync(() => cut.Instance.SetMessage("   "));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        // Assert
        wasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessage_TrimsMessage_BeforeSending()
    {
        // Arrange
        var sentMessage = string.Empty;
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string msg) => sentMessage = msg));

        await cut.InvokeAsync(() => cut.Instance.SetMessage("  Hello, world!  "));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        // Assert
        sentMessage.Should().Be("Hello, world!");
    }

    [Fact]
    public async Task HandleKeyDown_SendsMessage_OnEnterKey()
    {
        // Arrange
        var sentMessage = string.Empty;
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string msg) => sentMessage = msg));

        await cut.InvokeAsync(() => cut.Instance.SetMessage("Enter test"));

        // Simulate Enter key
        var enterKeyArgs = new KeyboardEventArgs { Key = "Enter", ShiftKey = false };

        // Act - use reflection to access the protected method for testing
        await cut.InvokeAsync(async () =>
        {
            var handleKeyDownMethod = typeof(MudMessageInput).GetMethod("HandleKeyDown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task?)handleKeyDownMethod?.Invoke(cut.Instance, new object[] { enterKeyArgs });
            if (task != null) await task;
        });

        // Assert
        sentMessage.Should().Be("Enter test");
    }

    [Fact]
    public async Task HandleKeyDown_DoesNotSendMessage_OnShiftEnterKey()
    {
        // Arrange
        var wasCalled = false;
        var cut = RenderComponent<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string _) => wasCalled = true));

        await cut.InvokeAsync(() => cut.Instance.SetMessage("Shift+Enter test"));

        // Simulate Shift+Enter key
        var shiftEnterKeyArgs = new KeyboardEventArgs { Key = "Enter", ShiftKey = true };

        // Act
        await cut.InvokeAsync(async () =>
        {
            var handleKeyDownMethod = typeof(MudMessageInput).GetMethod("HandleKeyDown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task?)handleKeyDownMethod?.Invoke(cut.Instance, new object[] { shiftEnterKeyArgs });
            if (task != null) await task;
        });

        // Assert
        wasCalled.Should().BeFalse();
        cut.Instance.Message.Should().Be("Shift+Enter test");
    }

    [Fact]
    public async Task Clear_ClearsMessage()
    {
        // Arrange
        var cut = RenderComponent<MudMessageInput>();
        await cut.InvokeAsync(() => cut.Instance.SetMessage("Test message"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.Clear());

        // Assert
        cut.Instance.Message.Should().BeEmpty();
    }

    [Fact]
    public async Task SetMessage_SetsMessageText()
    {
        // Arrange
        var cut = RenderComponent<MudMessageInput>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.SetMessage("New message"));

        // Assert
        cut.Instance.Message.Should().Be("New message");
    }

    [Fact]
    public void TextField_HasAutoGrowEnabled()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>();

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.AutoGrow.Should().BeTrue();
    }

    [Fact]
    public void TextField_HasMaxLines_SetToFive()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>();

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.MaxLines.Should().Be(5);
    }

    [Fact]
    public void TextField_HasOutlinedVariant()
    {
        // Act
        var cut = RenderComponent<MudMessageInput>();

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.Variant.Should().Be(Variant.Outlined);
    }
}
