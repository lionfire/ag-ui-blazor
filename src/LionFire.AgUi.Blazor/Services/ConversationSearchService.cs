using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// Default implementation of <see cref="IConversationSearchService"/>.
/// </summary>
public sealed class ConversationSearchService : IConversationSearchService
{
    /// <inheritdoc />
    public IReadOnlyDictionary<int, IReadOnlyList<MessageMatch>> SearchInConversation(
        Conversation conversation,
        string query,
        bool caseSensitive = false)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        if (string.IsNullOrWhiteSpace(query))
        {
            return new Dictionary<int, IReadOnlyList<MessageMatch>>();
        }

        var results = new Dictionary<int, IReadOnlyList<MessageMatch>>();
        var comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        for (int i = 0; i < conversation.Messages.Count; i++)
        {
            var message = conversation.Messages[i];
            var text = message.Text ?? string.Empty;
            var matches = FindMatches(text, query, comparison);

            if (matches.Count > 0)
            {
                results[i] = matches;
            }
        }

        return results;
    }

    /// <inheritdoc />
    public IReadOnlyList<ConversationSearchResult> SearchAcrossConversations(
        IEnumerable<Conversation> conversations,
        string query,
        bool caseSensitive = false)
    {
        ArgumentNullException.ThrowIfNull(conversations);

        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<ConversationSearchResult>();
        }

        var results = new List<ConversationSearchResult>();
        var comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        foreach (var conversation in conversations)
        {
            var matchingIndices = new List<int>();
            var totalMatchCount = 0;

            for (int i = 0; i < conversation.Messages.Count; i++)
            {
                var message = conversation.Messages[i];
                var text = message.Text ?? string.Empty;
                var matchCount = CountMatches(text, query, comparison);

                if (matchCount > 0)
                {
                    matchingIndices.Add(i);
                    totalMatchCount += matchCount;
                }
            }

            if (totalMatchCount > 0)
            {
                var metadata = ConversationMetadata.FromConversation(conversation);
                results.Add(new ConversationSearchResult(metadata, totalMatchCount, matchingIndices));
            }
        }

        // Sort by match count descending, then by last modified date descending
        return results
            .OrderByDescending(r => r.MatchCount)
            .ThenByDescending(r => r.Conversation.LastModifiedAt)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<ConversationMetadata> FilterByTags(
        IEnumerable<ConversationMetadata> conversations,
        IEnumerable<string> tags,
        bool matchAll = false)
    {
        ArgumentNullException.ThrowIfNull(conversations);
        ArgumentNullException.ThrowIfNull(tags);

        var tagSet = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);

        if (tagSet.Count == 0)
        {
            return conversations.ToList();
        }

        return conversations
            .Where(c => matchAll
                ? tagSet.All(t => c.Tags.Contains(t, StringComparer.OrdinalIgnoreCase))
                : tagSet.Any(t => c.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
            .ToList();
    }

    private static List<MessageMatch> FindMatches(string text, string query, StringComparison comparison)
    {
        var matches = new List<MessageMatch>();
        var index = 0;

        while ((index = text.IndexOf(query, index, comparison)) != -1)
        {
            matches.Add(new MessageMatch(index, query.Length));
            index += query.Length;
        }

        return matches;
    }

    private static int CountMatches(string text, string query, StringComparison comparison)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(query, index, comparison)) != -1)
        {
            count++;
            index += query.Length;
        }

        return count;
    }
}
