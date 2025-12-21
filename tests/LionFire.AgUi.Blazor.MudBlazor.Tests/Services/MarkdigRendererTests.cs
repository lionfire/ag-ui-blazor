using FluentAssertions;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Services;

/// <summary>
/// Unit tests for the MarkdigRenderer service.
/// </summary>
public class MarkdigRendererTests
{
    private readonly MarkdigRenderer _renderer;

    public MarkdigRendererTests()
    {
        _renderer = new MarkdigRenderer();
    }

    #region RenderHtml Tests

    [Fact]
    public void RenderHtml_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = _renderer.RenderHtml(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void RenderHtml_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = _renderer.RenderHtml(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void RenderHtml_WithWhitespace_ReturnsEmptyString()
    {
        // Act
        var result = _renderer.RenderHtml("   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void RenderHtml_WithHeading_RendersH1()
    {
        // Arrange
        var markdown = "# Hello World";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<h1");
        result.Should().Contain("Hello World");
        result.Should().Contain("</h1>");
    }

    [Fact]
    public void RenderHtml_WithHeadingLevel2_RendersH2()
    {
        // Arrange
        var markdown = "## Section Title";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<h2");
        result.Should().Contain("Section Title");
        result.Should().Contain("</h2>");
    }

    [Fact]
    public void RenderHtml_WithBoldText_RendersStrong()
    {
        // Arrange
        var markdown = "This is **bold** text";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<strong>");
        result.Should().Contain("bold");
        result.Should().Contain("</strong>");
    }

    [Fact]
    public void RenderHtml_WithItalicText_RendersEm()
    {
        // Arrange
        var markdown = "This is *italic* text";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<em>");
        result.Should().Contain("italic");
        result.Should().Contain("</em>");
    }

    [Fact]
    public void RenderHtml_WithUnorderedList_RendersUl()
    {
        // Arrange
        var markdown = "- Item 1\n- Item 2\n- Item 3";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<ul>");
        result.Should().Contain("<li>");
        result.Should().Contain("Item 1");
        result.Should().Contain("Item 2");
        result.Should().Contain("Item 3");
        result.Should().Contain("</ul>");
    }

    [Fact]
    public void RenderHtml_WithOrderedList_RendersOl()
    {
        // Arrange
        var markdown = "1. First\n2. Second\n3. Third";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<ol>");
        result.Should().Contain("<li>");
        result.Should().Contain("First");
        result.Should().Contain("Second");
        result.Should().Contain("Third");
        result.Should().Contain("</ol>");
    }

    [Fact]
    public void RenderHtml_WithLink_RendersAnchor()
    {
        // Arrange
        var markdown = "[Example](https://example.com)";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<a");
        result.Should().Contain("href=\"https://example.com\"");
        result.Should().Contain("Example");
        result.Should().Contain("</a>");
    }

    [Fact]
    public void RenderHtml_WithInlineCode_RendersCode()
    {
        // Arrange
        var markdown = "Use `Console.WriteLine()` for output";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<code>");
        result.Should().Contain("Console.WriteLine()");
        result.Should().Contain("</code>");
    }

    [Fact]
    public void RenderHtml_WithCodeBlock_RendersPreCode()
    {
        // Arrange
        var markdown = "```csharp\nvar x = 1;\n```";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<pre>");
        result.Should().Contain("<code");
        result.Should().Contain("var x = 1;");
        result.Should().Contain("</code>");
        result.Should().Contain("</pre>");
    }

    [Fact]
    public void RenderHtml_WithCodeBlock_AddsLanguageClass()
    {
        // Arrange
        var markdown = "```csharp\nvar x = 1;\n```";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("language-csharp");
    }

    [Fact]
    public void RenderHtml_WithCodeBlock_NormalizesLanguage()
    {
        // Arrange
        var markdown = "```cs\nvar x = 1;\n```";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("language-csharp");
    }

    [Fact]
    public void RenderHtml_WithJavaScriptCodeBlock_NormalizesLanguage()
    {
        // Arrange
        var markdown = "```js\nconst x = 1;\n```";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("language-javascript");
    }

    [Fact]
    public void RenderHtml_WithBlockquote_RendersBlockquote()
    {
        // Arrange
        var markdown = "> This is a quote";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<blockquote>");
        result.Should().Contain("This is a quote");
        result.Should().Contain("</blockquote>");
    }

    [Fact]
    public void RenderHtml_WithTable_RendersTable()
    {
        // Arrange
        var markdown = "| Name | Age |\n|------|-----|\n| John | 30  |";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<table>");
        result.Should().Contain("<th>");
        result.Should().Contain("<td>");
        result.Should().Contain("Name");
        result.Should().Contain("Age");
        result.Should().Contain("John");
        result.Should().Contain("30");
        result.Should().Contain("</table>");
    }

    [Fact]
    public void RenderHtml_WithTaskList_RendersCheckboxes()
    {
        // Arrange
        var markdown = "- [x] Completed task\n- [ ] Pending task";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("type=\"checkbox\"");
        result.Should().Contain("Completed task");
        result.Should().Contain("Pending task");
    }

    [Fact]
    public void RenderHtml_WithHorizontalRule_RendersHr()
    {
        // Arrange
        var markdown = "---";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<hr");
    }

    [Fact]
    public void RenderHtml_WithParagraph_RendersP()
    {
        // Arrange
        var markdown = "This is a paragraph.";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<p>");
        result.Should().Contain("This is a paragraph.");
        result.Should().Contain("</p>");
    }

    #endregion

    #region RenderMarkup Tests

    [Fact]
    public void RenderMarkup_ReturnsMarkupString()
    {
        // Arrange
        var markdown = "**Bold text**";

        // Act
        var result = _renderer.RenderMarkup(markdown);

        // Assert
        result.Value.Should().Contain("<strong>");
        result.Value.Should().Contain("Bold text");
    }

    [Fact]
    public void RenderMarkup_WithNull_ReturnsEmptyMarkupString()
    {
        // Act
        var result = _renderer.RenderMarkup(null);

        // Assert
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region ExtractCodeBlocks Tests

    [Fact]
    public void ExtractCodeBlocks_WithNull_ReturnsEmptyList()
    {
        // Act
        var result = _renderer.ExtractCodeBlocks(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractCodeBlocks_WithNoCodeBlocks_ReturnsEmptyList()
    {
        // Arrange
        var markdown = "Just some plain text";

        // Act
        var result = _renderer.ExtractCodeBlocks(markdown);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractCodeBlocks_WithFencedCodeBlock_ReturnsCodeBlock()
    {
        // Arrange
        var markdown = "```csharp\nvar x = 1;\n```";

        // Act
        var result = _renderer.ExtractCodeBlocks(markdown);

        // Assert
        result.Should().HaveCount(1);
        result[0].Code.Should().Contain("var x = 1;");
        result[0].Language.Should().Be("csharp");
    }

    [Fact]
    public void ExtractCodeBlocks_WithMultipleCodeBlocks_ReturnsAll()
    {
        // Arrange
        var markdown = "```csharp\nvar x = 1;\n```\n\nSome text\n\n```python\nprint('hello')\n```";

        // Act
        var result = _renderer.ExtractCodeBlocks(markdown);

        // Assert
        result.Should().HaveCount(2);
        result[0].Language.Should().Be("csharp");
        result[1].Language.Should().Be("python");
    }

    [Fact]
    public void ExtractCodeBlocks_WithNoLanguage_ReturnsNullLanguage()
    {
        // Arrange
        var markdown = "```\nsome code\n```";

        // Act
        var result = _renderer.ExtractCodeBlocks(markdown);

        // Assert
        result.Should().HaveCount(1);
        result[0].Language.Should().BeNull();
    }

    [Fact]
    public void ExtractCodeBlocks_NormalizesLanguage()
    {
        // Arrange
        var markdown = "```js\nconst x = 1;\n```";

        // Act
        var result = _renderer.ExtractCodeBlocks(markdown);

        // Assert
        result.Should().HaveCount(1);
        result[0].Language.Should().Be("javascript");
    }

    #endregion

    #region XSS Prevention Tests

    [Fact]
    public void RenderHtml_RemovesScriptTags()
    {
        // Arrange
        var markdown = "<script>alert('xss')</script>";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().NotContain("<script>");
        result.Should().NotContain("</script>");
    }

    [Fact]
    public void RenderHtml_RemovesJavascriptUrls()
    {
        // Arrange
        var markdown = "[Click me](javascript:alert('xss'))";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().NotContain("javascript:");
    }

    [Fact]
    public void RenderHtml_RemovesOnClickHandlers()
    {
        // Arrange
        var markdown = "<div onclick=\"alert('xss')\">Test</div>";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().NotContain("onclick=");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void RenderHtml_LargeContent_CompletesInReasonableTime()
    {
        // Arrange - Create large markdown content
        var paragraphs = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            paragraphs.Add($"## Section {i}\n\nThis is paragraph {i} with **bold** and *italic* text.\n\n```csharp\nvar x{i} = {i};\n```\n");
        }
        var markdown = string.Join("\n", paragraphs);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _renderer.RenderHtml(markdown);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "rendering should complete within 1 second");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void RenderHtml_WithNestedElements_RendersCorrectly()
    {
        // Arrange
        var markdown = "- Item with **bold** and *italic*\n  - Nested item";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("<ul>");
        result.Should().Contain("<strong>");
        result.Should().Contain("<em>");
    }

    [Fact]
    public void RenderHtml_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var markdown = "Special chars: & < > \" '";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().NotBeEmpty();
        // HTML entities should be handled
    }

    [Fact]
    public void RenderHtml_WithUnicodeCharacters_PreservesContent()
    {
        // Arrange
        var markdown = "Unicode: Hello World";

        // Act
        var result = _renderer.RenderHtml(markdown);

        // Assert
        result.Should().Contain("Hello World");
    }

    #endregion
}
