using FluentAssertions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Tests.Services;

public class ConversationExporterTests
{
    private readonly ConversationExporter _exporter = new();

    #region Export Tests

    [Fact]
    public void Export_ToJson_ReturnsValidJson()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _exporter.Export(conversation, ExportFormat.Json);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"agentName\"");
        result.Should().Contain("Test Agent");
    }

    [Fact]
    public void Export_ToMarkdown_ReturnsFormattedMarkdown()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _exporter.Export(conversation, ExportFormat.Markdown);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("# Conversation with Test Agent");
        result.Should().Contain("## You");
        result.Should().Contain("## Assistant");
        result.Should().Contain("Hello, how are you?");
        result.Should().Contain("I'm doing great!");
    }

    [Fact]
    public void Export_WithNullConversation_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _exporter.Export(null!, ExportFormat.Json);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("conversation");
    }

    [Fact]
    public void Export_WithInvalidFormat_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var act = () => _exporter.Export(conversation, (ExportFormat)999);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("format");
    }

    #endregion

    #region Import Tests

    [Fact]
    public void ImportFromJson_WithValidJson_ReturnsConversation()
    {
        // Arrange
        var conversation = CreateTestConversation();
        var json = _exporter.Export(conversation, ExportFormat.Json);

        // Act
        var result = _exporter.ImportFromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.AgentName.Should().Be("Test Agent");
        result.Messages.Should().HaveCount(2);
    }

    [Fact]
    public void ImportFromJson_WithEmptyContent_ThrowsArgumentException()
    {
        // Act
        var act = () => _exporter.ImportFromJson(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }

    [Fact]
    public void ImportFromJson_WithWhitespaceContent_ThrowsArgumentException()
    {
        // Act
        var act = () => _exporter.ImportFromJson("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }

    #endregion

    #region GetFileExtension Tests

    [Theory]
    [InlineData(ExportFormat.Json, ".json")]
    [InlineData(ExportFormat.Markdown, ".md")]
    public void GetFileExtension_ReturnsCorrectExtension(ExportFormat format, string expected)
    {
        // Act
        var result = _exporter.GetFileExtension(format);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetFileExtension_WithInvalidFormat_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _exporter.GetFileExtension((ExportFormat)999);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("format");
    }

    #endregion

    #region GetMimeType Tests

    [Theory]
    [InlineData(ExportFormat.Json, "application/json")]
    [InlineData(ExportFormat.Markdown, "text/markdown")]
    public void GetMimeType_ReturnsCorrectMimeType(ExportFormat format, string expected)
    {
        // Act
        var result = _exporter.GetMimeType(format);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetMimeType_WithInvalidFormat_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _exporter.GetMimeType((ExportFormat)999);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("format");
    }

    #endregion

    #region Helper Methods

    private static Conversation CreateTestConversation()
    {
        return Conversation.Create("Test Agent")
            .WithMessage(new ChatMessage(ChatRole.User, "Hello, how are you?"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "I'm doing great!"));
    }

    #endregion
}
