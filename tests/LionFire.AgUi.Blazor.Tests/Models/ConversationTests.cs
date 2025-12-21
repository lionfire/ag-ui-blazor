using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for the Conversation record.
/// </summary>
public class ConversationTests
{
    [Fact]
    public void Create_ShouldInitializeWithDefaults()
    {
        var conversation = Conversation.Create("test-agent");

        conversation.Id.Should().NotBeNullOrEmpty();
        conversation.AgentName.Should().Be("test-agent");
        conversation.Messages.Should().BeEmpty();
        conversation.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        conversation.LastModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        conversation.Metadata.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var conv1 = Conversation.Create("agent");
        var conv2 = Conversation.Create("agent");

        conv1.Id.Should().NotBe(conv2.Id);
    }

    [Fact]
    public void WithMessage_ShouldAddMessageAndUpdateTimestamp()
    {
        var conversation = Conversation.Create("agent");
        var originalTimestamp = conversation.LastModifiedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        var message = new ChatMessage(ChatRole.User, "Hello");
        var updated = conversation.WithMessage(message);

        updated.Messages.Should().HaveCount(1);
        updated.Messages[0].Text.Should().Be("Hello");
        updated.LastModifiedAt.Should().BeAfter(originalTimestamp);

        // Original should be unchanged (immutability)
        conversation.Messages.Should().BeEmpty();
    }

    [Fact]
    public void WithMessage_ShouldPreserveOtherProperties()
    {
        var conversation = Conversation.Create("agent");
        var message = new ChatMessage(ChatRole.User, "Hello");
        var updated = conversation.WithMessage(message);

        updated.Id.Should().Be(conversation.Id);
        updated.AgentName.Should().Be(conversation.AgentName);
        updated.CreatedAt.Should().Be(conversation.CreatedAt);
    }

    [Fact]
    public void Constructor_ShouldAcceptAllParameters()
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello"),
            new ChatMessage(ChatRole.Assistant, "Hi there!")
        };
        var metadata = new Dictionary<string, object> { ["key"] = "value" };
        var created = DateTimeOffset.UtcNow.AddDays(-1);
        var modified = DateTimeOffset.UtcNow;

        var conversation = new Conversation(
            Id: "test-id",
            AgentName: "agent",
            Messages: messages,
            CreatedAt: created,
            LastModifiedAt: modified,
            Metadata: metadata);

        conversation.Id.Should().Be("test-id");
        conversation.AgentName.Should().Be("agent");
        conversation.Messages.Should().HaveCount(2);
        conversation.CreatedAt.Should().Be(created);
        conversation.LastModifiedAt.Should().Be(modified);
        conversation.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void Equality_ShouldWorkForRecords()
    {
        var messages = new List<ChatMessage>().AsReadOnly();
        var now = DateTimeOffset.UtcNow;

        var conv1 = new Conversation("id", "agent", messages, now, now, null);
        var conv2 = new Conversation("id", "agent", messages, now, now, null);

        conv1.Should().Be(conv2);
    }
}
