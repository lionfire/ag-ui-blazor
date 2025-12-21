using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudTypingIndicator component.
/// </summary>
public class MudTypingIndicatorTests : TestContext
{
    public MudTypingIndicatorTests()
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
        var cut = RenderComponent<MudTypingIndicator>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-typing-indicator").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_ThreeDots()
    {
        // Act
        var cut = RenderComponent<MudTypingIndicator>();

        // Assert
        var dots = cut.FindAll(".mud-typing-indicator .dot");
        dots.Should().HaveCount(3);
    }

    [Fact]
    public void Component_Accepts_AdditionalClass()
    {
        // Arrange
        var customClass = "my-custom-typing";

        // Act
        var cut = RenderComponent<MudTypingIndicator>(parameters => parameters
            .Add(p => p.Class, customClass));

        // Assert
        var indicator = cut.Find(".mud-typing-indicator");
        indicator.ClassList.Should().Contain(customClass);
    }

    [Fact]
    public void Component_Accepts_AdditionalAttributes()
    {
        // Arrange & Act
        var cut = RenderComponent<MudTypingIndicator>(parameters => parameters
            .AddUnmatched("data-testid", "typing-indicator")
            .AddUnmatched("aria-label", "Assistant is typing"));

        // Assert
        var indicator = cut.Find(".mud-typing-indicator");
        indicator.GetAttribute("data-testid").Should().Be("typing-indicator");
        indicator.GetAttribute("aria-label").Should().Be("Assistant is typing");
    }
}
