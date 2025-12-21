using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudConnectionStatus component.
/// </summary>
public class MudConnectionStatusTests : TestContext
{
    public MudConnectionStatusTests()
    {
        // Add MudBlazor services required for rendering
        Services.AddMudServices();

        // Add JSInterop mocks for MudBlazor
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void Component_Renders_Successfully()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-connection-status").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_StatusDot()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>();

        // Assert
        var dot = cut.Find(".status-dot");
        dot.Should().NotBeNull();
    }

    #endregion

    #region Connected State Tests

    [Fact]
    public void Component_Shows_GreenDot_WhenConnected()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Connected));

        // Assert
        var dot = cut.Find(".status-dot");
        dot.ClassList.Should().Contain("dot-green");
    }

    [Fact]
    public void Component_Shows_ConnectedClass_WhenConnected()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Connected));

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.ClassList.Should().Contain("status-connected");
    }

    [Fact]
    public void Component_Shows_ConnectedTitle_WhenConnected()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Connected));

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.GetAttribute("title").Should().Be("Connected");
    }

    #endregion

    #region Connecting State Tests

    [Fact]
    public void Component_Shows_YellowDot_WhenConnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Connecting));

        // Assert
        var dot = cut.Find(".status-dot");
        dot.ClassList.Should().Contain("dot-yellow");
    }

    [Fact]
    public void Component_Shows_PulsingDot_WhenConnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Connecting));

        // Assert
        var dot = cut.Find(".status-dot");
        dot.ClassList.Should().Contain("dot-pulse");
    }

    [Fact]
    public void Component_Shows_ConnectingTitle_WhenConnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Connecting));

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.GetAttribute("title").Should().Be("Connecting...");
    }

    #endregion

    #region Reconnecting State Tests

    [Fact]
    public void Component_Shows_YellowDot_WhenReconnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Reconnecting));

        // Assert
        var dot = cut.Find(".status-dot");
        dot.ClassList.Should().Contain("dot-yellow");
    }

    [Fact]
    public void Component_Shows_PulsingDot_WhenReconnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Reconnecting));

        // Assert
        var dot = cut.Find(".status-dot");
        dot.ClassList.Should().Contain("dot-pulse");
    }

    [Fact]
    public void Component_Shows_ReconnectingTitle_WhenReconnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Reconnecting));

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.GetAttribute("title").Should().Be("Reconnecting...");
    }

    #endregion

    #region Disconnected State Tests

    [Fact]
    public void Component_Shows_RedDot_WhenDisconnected()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Disconnected));

        // Assert
        var dot = cut.Find(".status-dot");
        dot.ClassList.Should().Contain("dot-red");
    }

    [Fact]
    public void Component_Shows_DisconnectedTitle_WhenDisconnected()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Disconnected));

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.GetAttribute("title").Should().Be("Disconnected");
    }

    #endregion

    #region Error State Tests

    [Fact]
    public void Component_Shows_RedDot_WhenError()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Error));

        // Assert
        var dot = cut.Find(".status-dot");
        dot.ClassList.Should().Contain("dot-red");
    }

    [Fact]
    public void Component_Shows_ErrorTitle_WhenError()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.State, ConnectionState.Error));

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.GetAttribute("title").Should().Be("Connection Error");
    }

    #endregion

    #region Default State Tests

    [Fact]
    public void Component_DefaultsTo_Disconnected()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>();

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.ClassList.Should().Contain("status-disconnected");
    }

    #endregion

    #region Label Tests

    [Fact]
    public void Component_HidesLabel_ByDefault()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>();

        // Assert
        var labels = cut.FindAll(".status-label");
        labels.Should().BeEmpty();
    }

    [Fact]
    public void Component_ShowsLabel_WhenEnabled()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.ShowLabel, true)
            .Add(p => p.State, ConnectionState.Connected));

        // Assert
        var label = cut.Find(".status-label");
        label.Should().NotBeNull();
        label.TextContent.Should().Be("Connected");
    }

    [Fact]
    public void Component_ShowsConnectingLabel_WhenConnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.ShowLabel, true)
            .Add(p => p.State, ConnectionState.Connecting));

        // Assert
        var label = cut.Find(".status-label");
        label.TextContent.Should().Be("Connecting...");
    }

    [Fact]
    public void Component_ShowsReconnectingLabel_WhenReconnecting()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.ShowLabel, true)
            .Add(p => p.State, ConnectionState.Reconnecting));

        // Assert
        var label = cut.Find(".status-label");
        label.TextContent.Should().Be("Reconnecting...");
    }

    [Fact]
    public void Component_ShowsDisconnectedLabel_WhenDisconnected()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.ShowLabel, true)
            .Add(p => p.State, ConnectionState.Disconnected));

        // Assert
        var label = cut.Find(".status-label");
        label.TextContent.Should().Be("Disconnected");
    }

    [Fact]
    public void Component_ShowsErrorLabel_WhenError()
    {
        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.ShowLabel, true)
            .Add(p => p.State, ConnectionState.Error));

        // Assert
        var label = cut.Find(".status-label");
        label.TextContent.Should().Be("Connection Error");
    }

    #endregion

    #region Class Parameter Tests

    [Fact]
    public void Component_Accepts_AdditionalClass()
    {
        // Arrange
        var customClass = "my-custom-status";

        // Act
        var cut = RenderComponent<MudConnectionStatus>(parameters => parameters
            .Add(p => p.Class, customClass)
            .Add(p => p.State, ConnectionState.Connected));

        // Assert
        var container = cut.Find(".mud-connection-status");
        container.ClassList.Should().Contain(customClass);
    }

    #endregion
}
