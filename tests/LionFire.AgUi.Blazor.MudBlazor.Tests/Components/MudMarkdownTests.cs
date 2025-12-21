using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using LionFire.AgUi.Blazor.MudBlazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Moq;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudMarkdown component.
/// </summary>
public class MudMarkdownTests : TestContext
{
    public MudMarkdownTests()
    {
        // Add required services
        Services.AddMudServices();
        Services.AddSingleton<IMarkdownRenderer, MarkdigRenderer>();

        // Add JSInterop mocks
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Component_Renders_Successfully()
    {
        // Act
        var cut = RenderComponent<MudMarkdown>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-markdown").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_WithEmptyContent()
    {
        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, string.Empty));

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-markdown").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_WithNullContent()
    {
        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, null));

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-markdown").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_MarkdownContent()
    {
        // Arrange
        var markdown = "**Bold text**";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, markdown));

        // Assert
        cut.Markup.Should().Contain("<strong>");
        cut.Markup.Should().Contain("Bold text");
        cut.Markup.Should().Contain("</strong>");
    }

    [Fact]
    public void Component_Renders_Headings()
    {
        // Arrange
        var markdown = "# Heading 1\n## Heading 2";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, markdown));

        // Assert
        cut.Markup.Should().Contain("<h1");
        cut.Markup.Should().Contain("Heading 1");
        cut.Markup.Should().Contain("<h2");
        cut.Markup.Should().Contain("Heading 2");
    }

    [Fact]
    public void Component_Renders_Lists()
    {
        // Arrange
        var markdown = "- Item 1\n- Item 2";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, markdown));

        // Assert
        cut.Markup.Should().Contain("<ul>");
        cut.Markup.Should().Contain("<li>");
        cut.Markup.Should().Contain("Item 1");
        cut.Markup.Should().Contain("Item 2");
    }

    [Fact]
    public void Component_Renders_CodeBlocks()
    {
        // Arrange
        var markdown = "```csharp\nvar x = 1;\n```";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, markdown));

        // Assert
        cut.Markup.Should().Contain("<pre>");
        cut.Markup.Should().Contain("<code");
        cut.Markup.Should().Contain("var x = 1;");
    }

    [Fact]
    public void Component_Applies_CustomClass()
    {
        // Arrange
        var customClass = "my-custom-class";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Class, customClass));

        // Assert
        cut.Find(".mud-markdown").ClassList.Should().Contain(customClass);
    }

    [Fact]
    public void Component_ShowCopyButton_DefaultsToTrue()
    {
        // Act
        var cut = RenderComponent<MudMarkdown>();

        // Assert
        cut.Instance.ShowCopyButton.Should().BeTrue();
    }

    [Fact]
    public void Component_EnableSyntaxHighlighting_DefaultsToTrue()
    {
        // Act
        var cut = RenderComponent<MudMarkdown>();

        // Assert
        cut.Instance.EnableSyntaxHighlighting.Should().BeTrue();
    }

    [Fact]
    public void Component_ReRendersOnContentChange()
    {
        // Arrange
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, "**Initial**"));

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Content, "**Updated**"));

        // Assert
        cut.Markup.Should().Contain("Updated");
        cut.Markup.Should().NotContain("Initial");
    }

    [Fact]
    public void Component_AppliesAdditionalAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, object>
        {
            { "data-testid", "test-markdown" }
        };

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.AdditionalAttributes, attributes));

        // Assert
        cut.Find(".mud-markdown").GetAttribute("data-testid").Should().Be("test-markdown");
    }

    [Fact]
    public void Component_Renders_Links()
    {
        // Arrange
        var markdown = "[Example](https://example.com)";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, markdown));

        // Assert
        cut.Markup.Should().Contain("<a");
        cut.Markup.Should().Contain("href=\"https://example.com\"");
        cut.Markup.Should().Contain("Example");
    }

    [Fact]
    public void Component_Renders_Tables()
    {
        // Arrange
        var markdown = "| Name | Age |\n|------|-----|\n| John | 30  |";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, markdown));

        // Assert
        cut.Markup.Should().Contain("<table>");
        cut.Markup.Should().Contain("<th>");
        cut.Markup.Should().Contain("<td>");
    }

    [Fact]
    public void Component_Renders_Blockquotes()
    {
        // Arrange
        var markdown = "> This is a quote";

        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, markdown));

        // Assert
        cut.Markup.Should().Contain("<blockquote>");
        cut.Markup.Should().Contain("This is a quote");
    }

    [Fact]
    public void Component_WithMockedRenderer_UsesInjectedService()
    {
        // Arrange
        var mockRenderer = new Mock<IMarkdownRenderer>();
        mockRenderer
            .Setup(r => r.RenderMarkup(It.IsAny<string>()))
            .Returns(new MarkupString("<p>Mocked content</p>"));

        // Create a new test context with mocked renderer
        using var ctx = new TestContext();
        ctx.Services.AddMudServices();
        ctx.Services.AddSingleton(mockRenderer.Object);
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, "Any content"));

        // Assert
        cut.Markup.Should().Contain("Mocked content");
        mockRenderer.Verify(r => r.RenderMarkup(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Component_DisposeAsync_CompletesSuccessfully()
    {
        // Arrange
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.Content, "Test content"));

        // Act & Assert - Should not throw
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.DisposeAsync();
        });
    }

    [Fact]
    public void Component_OnCodeCopied_EventCanBeSet()
    {
        // Act
        var cut = RenderComponent<MudMarkdown>(parameters => parameters
            .Add(p => p.OnCodeCopied, EventCallback.Factory.Create<string>(this, _ => { })));

        // Assert
        cut.Instance.Should().NotBeNull();
        // The event callback is properly set (would be invoked when copy is triggered)
    }
}
