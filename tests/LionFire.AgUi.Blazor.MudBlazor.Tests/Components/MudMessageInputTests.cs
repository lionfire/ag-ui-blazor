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
public class MudMessageInputTests : BunitContext, IAsyncLifetime
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
        var cut = Render<MudMessageInput>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-message-input").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_WithDefaultPlaceholder()
    {
        // Act
        var cut = Render<MudMessageInput>();

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
        var cut = Render<MudMessageInput>(parameters => parameters
            .Add(p => p.Placeholder, placeholder));

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.Placeholder.Should().Be(placeholder);
    }

    [Fact]
    public void Component_Renders_SendButton()
    {
        // Act
        var cut = Render<MudMessageInput>();

        // Assert
        var iconButton = cut.FindComponent<MudIconButton>();
        iconButton.Should().NotBeNull();
    }

    [Fact]
    public void SendButton_IsDisabled_WhenMessageIsEmpty()
    {
        // Act
        var cut = Render<MudMessageInput>();

        // Assert
        var iconButton = cut.FindComponent<MudIconButton>();
        iconButton.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public void SendButton_IsDisabled_WhenComponentIsDisabled()
    {
        // Act
        var cut = Render<MudMessageInput>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        var iconButton = cut.FindComponent<MudIconButton>();
        iconButton.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public void TextField_IsDisabled_WhenComponentIsDisabled()
    {
        // Act
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>(parameters => parameters
            .Add(p => p.OnSend, (string msg) => sentMessage = msg));

        await cut.InvokeAsync(() => cut.Instance.SetMessage("Enter test"));

        // Act - Enter (without Shift) is routed from JS interop to HandleEnterPressed
        await cut.InvokeAsync(() => cut.Instance.HandleEnterPressed());

        // Assert
        sentMessage.Should().Be("Enter test");
    }

    [Fact]
    public async Task HandleKeyDown_DoesNotSendMessage_OnShiftEnterKey()
    {
        // Arrange
        var wasCalled = false;
        var cut = Render<MudMessageInput>(parameters => parameters
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
        var cut = Render<MudMessageInput>();
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
        var cut = Render<MudMessageInput>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.SetMessage("New message"));

        // Assert
        cut.Instance.Message.Should().Be("New message");
    }

    [Fact]
    public void TextField_HasAutoGrowEnabled()
    {
        // Act
        var cut = Render<MudMessageInput>();

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.AutoGrow.Should().BeTrue();
    }

    [Fact]
    public void TextField_HasMaxLines_SetToFive()
    {
        // Act
        var cut = Render<MudMessageInput>();

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.MaxLines.Should().Be(5);
    }

    [Fact]
    public void TextField_HasOutlinedVariant()
    {
        // Act
        var cut = Render<MudMessageInput>();

        // Assert
        var textField = cut.FindComponent<MudTextField<string>>();
        textField.Instance.Variant.Should().Be(Variant.Outlined);
    }

    [Fact]
    public void FooterSlots_Null_PreservesClassicSingleRowLayout()
    {
        // Act
        var cut = Render<MudMessageInput>();

        // Assert - no footer row, no footer variant class
        cut.FindAll(".mud-message-input-footer").Should().BeEmpty();
        cut.Find(".mud-message-input").ClassList.Should().NotContain("has-footer");
        // Send button still present in the classic row
        cut.FindAll("button").Should().NotBeEmpty();
    }

    [Fact]
    public void FooterStart_Provided_RendersFooterRowWithContentAndSendButton()
    {
        // Act
        var cut = Render<MudMessageInput>(parameters => parameters
            .Add(p => p.FooterStart, builder => builder.AddMarkupContent(0, "<span id='model-picker-stub'>model</span>")));

        // Assert
        cut.Find(".mud-message-input").ClassList.Should().Contain("has-footer");
        cut.Find(".mud-message-input-footer").Should().NotBeNull();
        cut.Find("#model-picker-stub").TextContent.Should().Be("model");
        // Send button lives in the footer row now
        cut.Find(".mud-message-input-footer button[aria-label='Send message']").Should().NotBeNull();
    }

    [Fact]
    public void FooterEnd_Provided_RendersRightAlignedBeforeSendButton()
    {
        // Act
        var cut = Render<MudMessageInput>(parameters => parameters
            .Add(p => p.FooterEnd, builder => builder.AddMarkupContent(0, "<span id='settings-stub'>settings</span>")));

        // Assert
        var footerEnd = cut.Find(".mud-message-input-footer-end");
        footerEnd.QuerySelector("#settings-stub").Should().NotBeNull();
    }

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;
    async Task IAsyncLifetime.DisposeAsync() => await base.DisposeAsync();
}
