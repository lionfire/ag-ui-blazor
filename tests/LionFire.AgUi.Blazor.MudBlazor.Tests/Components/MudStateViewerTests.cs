using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

public class MudStateViewerTests : TestContext
{
    public MudStateViewerTests()
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
        var cut = RenderComponent<MudStateViewer>();

        // Assert
        cut.Markup.Should().Contain("mud-state-viewer");
        cut.Markup.Should().Contain("State Viewer");
    }

    [Fact]
    public void Renders_WithCustomTitle()
    {
        // Act
        var cut = RenderComponent<MudStateViewer>(parameters => parameters
            .Add(p => p.Title, "Custom State"));

        // Assert
        cut.Markup.Should().Contain("Custom State");
    }

    [Fact]
    public void Renders_EmptyMessage_WhenNoState()
    {
        // Act
        var cut = RenderComponent<MudStateViewer>(parameters => parameters
            .Add(p => p.EmptyMessage, "No data"));

        // Assert
        cut.Markup.Should().Contain("No data");
    }

    [Fact]
    public void Renders_StateTable_WhenStateProvided()
    {
        // Arrange
        var state = new Dictionary<string, object?>
        {
            { "key1", "value1" },
            { "key2", 42 }
        };

        // Act
        var cut = RenderComponent<MudStateViewer>(parameters => parameters
            .Add(p => p.State, state));

        // Assert
        cut.Markup.Should().Contain("key1");
        cut.Markup.Should().Contain("value1");
        cut.Markup.Should().Contain("key2");
        cut.Markup.Should().Contain("42");
    }

    #endregion

    #region Last Updated Tests

    [Fact]
    public void Renders_LastUpdated_WhenProvided()
    {
        // Act
        var cut = RenderComponent<MudStateViewer>(parameters => parameters
            .Add(p => p.LastUpdated, "2025-01-01 12:00:00"));

        // Assert
        cut.Markup.Should().Contain("Last updated:");
        cut.Markup.Should().Contain("2025-01-01 12:00:00");
    }

    [Fact]
    public void DoesNotRender_LastUpdated_WhenNull()
    {
        // Act
        var cut = RenderComponent<MudStateViewer>(parameters => parameters
            .Add(p => p.LastUpdated, null));

        // Assert
        cut.Markup.Should().NotContain("Last updated:");
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void ShowRefreshButton_DefaultsToTrue()
    {
        // Act
        var cut = RenderComponent<MudStateViewer>();

        // Assert
        cut.Instance.ShowRefreshButton.Should().BeTrue();
    }

    [Fact]
    public void ShowExpandAllButton_DefaultsToTrue()
    {
        // Act
        var cut = RenderComponent<MudStateViewer>();

        // Assert
        cut.Instance.ShowExpandAllButton.Should().BeTrue();
    }

    #endregion
}
