using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for the ConversationMetadata record.
/// </summary>
public class ConversationMetadataTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var tags = new List<string> { "important", "work" };
        var created = DateTimeOffset.UtcNow.AddDays(-1);
        var modified = DateTimeOffset.UtcNow;

        var metadata = new ConversationMetadata(
            Id: "conv-123",
            AgentName: "assistant",
            Title: "Project Discussion",
            CreatedAt: created,
            LastModifiedAt: modified,
            MessageCount: 42,
            Tags: tags);

        metadata.Id.Should().Be("conv-123");
        metadata.AgentName.Should().Be("assistant");
        metadata.Title.Should().Be("Project Discussion");
        metadata.CreatedAt.Should().Be(created);
        metadata.LastModifiedAt.Should().Be(modified);
        metadata.MessageCount.Should().Be(42);
        metadata.Tags.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public void FromConversation_ShouldExtractMetadata()
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello"),
            new ChatMessage(ChatRole.Assistant, "Hi!")
        };
        var conversation = new Conversation(
            Id: "conv-456",
            AgentName: "agent",
            Messages: messages,
            CreatedAt: DateTimeOffset.UtcNow.AddHours(-1),
            LastModifiedAt: DateTimeOffset.UtcNow,
            Metadata: null);

        var tags = new List<string> { "test" };
        var metadata = ConversationMetadata.FromConversation(conversation, "Custom Title", tags);

        metadata.Id.Should().Be(conversation.Id);
        metadata.AgentName.Should().Be(conversation.AgentName);
        metadata.Title.Should().Be("Custom Title");
        metadata.CreatedAt.Should().Be(conversation.CreatedAt);
        metadata.LastModifiedAt.Should().Be(conversation.LastModifiedAt);
        metadata.MessageCount.Should().Be(2);
        metadata.Tags.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public void FromConversation_WithDefaults_ShouldUseEmptyTags()
    {
        var conversation = Conversation.Create("agent");
        var metadata = ConversationMetadata.FromConversation(conversation);

        metadata.Title.Should().BeNull();
        metadata.Tags.Should().BeEmpty();
        metadata.MessageCount.Should().Be(0);
    }

    [Fact]
    public void JsonSerialization_ShouldRoundTrip()
    {
        var original = new ConversationMetadata(
            Id: "json-conv",
            AgentName: "agent",
            Title: "Test Title",
            CreatedAt: DateTimeOffset.Parse("2024-01-15T10:30:00Z"),
            LastModifiedAt: DateTimeOffset.Parse("2024-01-15T11:00:00Z"),
            MessageCount: 10,
            Tags: new List<string> { "tag1", "tag2" });

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ConversationMetadata>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(original.Id);
        deserialized.Title.Should().Be(original.Title);
        deserialized.MessageCount.Should().Be(original.MessageCount);
    }

    [Fact]
    public void Equality_ShouldWorkForRecords()
    {
        var tags = new List<string> { "tag" }.AsReadOnly();
        var now = DateTimeOffset.UtcNow;

        var meta1 = new ConversationMetadata("id", "agent", "title", now, now, 5, tags);
        var meta2 = new ConversationMetadata("id", "agent", "title", now, now, 5, tags);

        meta1.Should().Be(meta2);
    }
}
