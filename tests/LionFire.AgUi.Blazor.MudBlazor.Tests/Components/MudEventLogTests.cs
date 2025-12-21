using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

public class MudEventLogTests : TestContext
{
    public MudEventLogTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        RenderComponent<MudPopoverProvider>();
    }

    #region Rendering Tests

    [Fact]
    public void Renders_WithDefaultParameters()
    {
        // Act
        var cut = RenderComponent<MudEventLog>();

        // Assert
        cut.Markup.Should().Contain("mud-event-log");
        cut.Markup.Should().Contain("Event Log");
    }

    [Fact]
    public void Renders_WithCustomTitle()
    {
        // Act
        var cut = RenderComponent<MudEventLog>(parameters => parameters
            .Add(p => p.Title, "Custom Log"));

        // Assert
        cut.Markup.Should().Contain("Custom Log");
    }

    [Fact]
    public void Renders_WithEmptyMessage_WhenNoEvents()
    {
        // Act
        var cut = RenderComponent<MudEventLog>(parameters => parameters
            .Add(p => p.EmptyMessage, "No events yet"));

        // Assert
        cut.Markup.Should().Contain("No events yet");
    }

    [Fact]
    public void Renders_EventList_WhenEventsProvided()
    {
        // Arrange
        var events = new List<EventLogEntry>
        {
            new(DateTimeOffset.UtcNow, "Test", "Test message")
        };

        // Act
        var cut = RenderComponent<MudEventLog>(parameters => parameters
            .Add(p => p.Events, events));

        // Assert
        cut.Markup.Should().Contain("Test");
        cut.Markup.Should().Contain("Test message");
    }

    #endregion

    #region Event Level Tests

    [Theory]
    [InlineData(EventLogLevel.Info, "event-info")]
    [InlineData(EventLogLevel.Warning, "event-warning")]
    [InlineData(EventLogLevel.Error, "event-error")]
    [InlineData(EventLogLevel.Debug, "event-debug")]
    public void Renders_EventWithCorrectClass(EventLogLevel level, string expectedClass)
    {
        // Arrange
        var events = new List<EventLogEntry>
        {
            new(DateTimeOffset.UtcNow, "Test", "Message", level)
        };

        // Act
        var cut = RenderComponent<MudEventLog>(parameters => parameters
            .Add(p => p.Events, events));

        // Assert
        cut.Markup.Should().Contain(expectedClass);
    }

    #endregion

    #region Clear Button Tests

    [Fact]
    public void ClearButton_IsVisible_WhenEventsPresent()
    {
        // Arrange
        var events = new List<EventLogEntry>
        {
            new(DateTimeOffset.UtcNow, "Test", "Message")
        };

        // Act
        var cut = RenderComponent<MudEventLog>(parameters => parameters
            .Add(p => p.Events, events)
            .Add(p => p.ShowClearButton, true));

        // Assert
        cut.FindAll("button").Should().NotBeEmpty();
    }

    [Fact]
    public void ClearButton_IsHidden_WhenDisabled()
    {
        // Arrange
        var events = new List<EventLogEntry>
        {
            new(DateTimeOffset.UtcNow, "Test", "Message")
        };

        // Act
        var cut = RenderComponent<MudEventLog>(parameters => parameters
            .Add(p => p.Events, events)
            .Add(p => p.ShowClearButton, false));

        // Assert - The title attribute for clear button should not be present
        cut.Markup.Should().NotContain("Clear events");
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void MaxVisibleEvents_DefaultsTo100()
    {
        // Act
        var cut = RenderComponent<MudEventLog>();

        // Assert
        cut.Instance.MaxVisibleEvents.Should().Be(100);
    }

    [Fact]
    public void ShowClearButton_DefaultsToTrue()
    {
        // Act
        var cut = RenderComponent<MudEventLog>();

        // Assert
        cut.Instance.ShowClearButton.Should().BeTrue();
    }

    #endregion
}
