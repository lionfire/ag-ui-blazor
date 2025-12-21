using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// Represents a search result for a conversation.
/// </summary>
/// <param name="Conversation">The matching conversation metadata.</param>
/// <param name="MatchCount">The number of matches found.</param>
/// <param name="MatchingMessageIndices">The indices of messages that contain matches.</param>
public sealed record ConversationSearchResult(
    ConversationMetadata Conversation,
    int MatchCount,
    IReadOnlyList<int> MatchingMessageIndices
);

/// <summary>
/// Represents a match within a message.
/// </summary>
/// <param name="StartIndex">The start index of the match in the message text.</param>
/// <param name="Length">The length of the match.</param>
public sealed record MessageMatch(int StartIndex, int Length);

/// <summary>
/// Service for searching within and across conversations.
/// </summary>
public interface IConversationSearchService
{
    /// <summary>
    /// Searches within a single conversation's messages.
    /// </summary>
    /// <param name="conversation">The conversation to search.</param>
    /// <param name="query">The search query.</param>
    /// <param name="caseSensitive">Whether the search is case-sensitive.</param>
    /// <returns>A dictionary mapping message indices to their matches.</returns>
    IReadOnlyDictionary<int, IReadOnlyList<MessageMatch>> SearchInConversation(
        Conversation conversation,
        string query,
        bool caseSensitive = false);

    /// <summary>
    /// Searches across multiple conversations.
    /// </summary>
    /// <param name="conversations">The conversations to search.</param>
    /// <param name="query">The search query.</param>
    /// <param name="caseSensitive">Whether the search is case-sensitive.</param>
    /// <returns>A list of search results ordered by relevance.</returns>
    IReadOnlyList<ConversationSearchResult> SearchAcrossConversations(
        IEnumerable<Conversation> conversations,
        string query,
        bool caseSensitive = false);

    /// <summary>
    /// Filters conversations by tags.
    /// </summary>
    /// <param name="conversations">The conversations to filter.</param>
    /// <param name="tags">The tags to filter by.</param>
    /// <param name="matchAll">Whether all tags must match (AND) or any tag (OR).</param>
    /// <returns>The filtered conversation metadata.</returns>
    IReadOnlyList<ConversationMetadata> FilterByTags(
        IEnumerable<ConversationMetadata> conversations,
        IEnumerable<string> tags,
        bool matchAll = false);
}
