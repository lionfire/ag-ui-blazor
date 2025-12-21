using FluentAssertions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Tests.Services;

public class ConversationSearchServiceTests
{
    private readonly ConversationSearchService _service = new();

    #region SearchInConversation Tests

    [Fact]
    public void SearchInConversation_FindsMatchesInMessages()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _service.SearchInConversation(conversation, "hello");

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(0);
        result[0].Should().HaveCount(1);
        result[0][0].StartIndex.Should().Be(0);
        result[0][0].Length.Should().Be(5);
    }

    [Fact]
    public void SearchInConversation_IsCaseInsensitiveByDefault()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _service.SearchInConversation(conversation, "HELLO");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void SearchInConversation_CaseSensitiveWhenSpecified()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _service.SearchInConversation(conversation, "HELLO", caseSensitive: true);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchInConversation_FindsMultipleMatchesInSameMessage()
    {
        // Arrange
        var conversation = Conversation.Create("Agent")
            .WithMessage(new ChatMessage(ChatRole.User, "test test test"));

        // Act
        var result = _service.SearchInConversation(conversation, "test");

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3);
    }

    [Fact]
    public void SearchInConversation_ReturnsEmptyForNoMatches()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _service.SearchInConversation(conversation, "nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchInConversation_WithEmptyQuery_ReturnsEmpty()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _service.SearchInConversation(conversation, "");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchInConversation_WithWhitespaceQuery_ReturnsEmpty()
    {
        // Arrange
        var conversation = CreateTestConversation();

        // Act
        var result = _service.SearchInConversation(conversation, "   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchInConversation_WithNullConversation_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.SearchInConversation(null!, "test");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("conversation");
    }

    #endregion

    #region SearchAcrossConversations Tests

    [Fact]
    public void SearchAcrossConversations_FindsMatchingConversations()
    {
        // Arrange
        var conversations = CreateMultipleConversations();

        // Act
        var result = _service.SearchAcrossConversations(conversations, "hello");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void SearchAcrossConversations_OrdersByMatchCountDescending()
    {
        // Arrange
        var conversations = CreateMultipleConversations();

        // Act
        var result = _service.SearchAcrossConversations(conversations, "test");

        // Assert
        result.Should().HaveCount(2);
        result[0].MatchCount.Should().BeGreaterThanOrEqualTo(result[1].MatchCount);
    }

    [Fact]
    public void SearchAcrossConversations_ReturnsMatchingMessageIndices()
    {
        // Arrange
        var conversations = CreateMultipleConversations();

        // Act
        var result = _service.SearchAcrossConversations(conversations, "hello");

        // Assert
        result.Should().Contain(r => r.MatchingMessageIndices.Count > 0);
    }

    [Fact]
    public void SearchAcrossConversations_WithEmptyQuery_ReturnsEmpty()
    {
        // Arrange
        var conversations = CreateMultipleConversations();

        // Act
        var result = _service.SearchAcrossConversations(conversations, "");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchAcrossConversations_WithNullConversations_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.SearchAcrossConversations(null!, "test");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("conversations");
    }

    #endregion

    #region FilterByTags Tests

    [Fact]
    public void FilterByTags_FiltersConversationsByTag()
    {
        // Arrange
        var conversations = CreateConversationsWithTags();

        // Act
        var result = _service.FilterByTags(conversations, new[] { "important" });

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Tags.Contains("important", StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void FilterByTags_MatchAll_RequiresAllTags()
    {
        // Arrange
        var conversations = CreateConversationsWithTags();

        // Act
        var result = _service.FilterByTags(conversations, new[] { "important", "archived" }, matchAll: true);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void FilterByTags_MatchAny_MatchesAnyTag()
    {
        // Arrange
        var conversations = CreateConversationsWithTags();

        // Act
        var result = _service.FilterByTags(conversations, new[] { "important", "archived" }, matchAll: false);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void FilterByTags_WithEmptyTags_ReturnsAllConversations()
    {
        // Arrange
        var conversations = CreateConversationsWithTags();

        // Act
        var result = _service.FilterByTags(conversations, Array.Empty<string>());

        // Assert
        result.Should().HaveCount(conversations.Count);
    }

    [Fact]
    public void FilterByTags_IsCaseInsensitive()
    {
        // Arrange
        var conversations = CreateConversationsWithTags();

        // Act
        var result = _service.FilterByTags(conversations, new[] { "IMPORTANT" });

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void FilterByTags_WithNullConversations_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.FilterByTags(null!, new[] { "tag" });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("conversations");
    }

    [Fact]
    public void FilterByTags_WithNullTags_ThrowsArgumentNullException()
    {
        // Arrange
        var conversations = CreateConversationsWithTags();

        // Act
        var act = () => _service.FilterByTags(conversations, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("tags");
    }

    #endregion

    #region Helper Methods

    private static Conversation CreateTestConversation()
    {
        return Conversation.Create("Test Agent")
            .WithMessage(new ChatMessage(ChatRole.User, "Hello, how are you?"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "I'm doing great!"));
    }

    private static List<Conversation> CreateMultipleConversations()
    {
        var conv1 = Conversation.Create("Agent 1")
            .WithMessage(new ChatMessage(ChatRole.User, "Hello there"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "Hi! This is a test response."));

        var conv2 = Conversation.Create("Agent 2")
            .WithMessage(new ChatMessage(ChatRole.User, "Hello again"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "Test test test"));

        var conv3 = Conversation.Create("Agent 3")
            .WithMessage(new ChatMessage(ChatRole.User, "Goodbye"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "See you later!"));

        return new List<Conversation> { conv1, conv2, conv3 };
    }

    private static List<ConversationMetadata> CreateConversationsWithTags()
    {
        var now = DateTimeOffset.UtcNow;

        return new List<ConversationMetadata>
        {
            new("1", "Agent 1", null, now, now, 5, new List<string> { "important" }),
            new("2", "Agent 2", null, now, now, 3, new List<string> { "archived" }),
            new("3", "Agent 3", null, now, now, 2, new List<string> { "important", "archived" }),
            new("4", "Agent 4", null, now, now, 1, new List<string>())
        };
    }

    #endregion
}
