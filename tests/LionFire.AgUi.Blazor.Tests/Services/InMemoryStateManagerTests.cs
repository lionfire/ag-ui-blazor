using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Tests.Services;

public class InMemoryStateManagerTests
{
    private readonly InMemoryStateManager _sut;

    public InMemoryStateManagerTests()
    {
        _sut = new InMemoryStateManager();
    }

    [Fact]
    public async Task SaveConversationAsync_SavesConversation()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent");

        // Act
        await _sut.SaveConversationAsync(conversation);

        // Assert
        Assert.Equal(1, _sut.Count);
    }

    [Fact]
    public async Task SaveConversationAsync_ThrowsOnNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.SaveConversationAsync(null!));
    }

    [Fact]
    public async Task LoadConversationAsync_ReturnsConversation()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent");
        await _sut.SaveConversationAsync(conversation);

        // Act
        var loaded = await _sut.LoadConversationAsync(conversation.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(conversation.Id, loaded.Id);
        Assert.Equal(conversation.AgentName, loaded.AgentName);
    }

    [Fact]
    public async Task LoadConversationAsync_ReturnsNullWhenNotFound()
    {
        // Act
        var loaded = await _sut.LoadConversationAsync("non-existent");

        // Assert
        Assert.Null(loaded);
    }

    [Fact]
    public async Task LoadConversationAsync_ThrowsOnNullOrWhitespace()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.LoadConversationAsync(null!));
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.LoadConversationAsync(""));
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.LoadConversationAsync("   "));
    }

    [Fact]
    public async Task ListConversationsAsync_ReturnsAllConversations()
    {
        // Arrange
        var conv1 = Conversation.Create("agent1");
        var conv2 = Conversation.Create("agent2");
        var conv3 = Conversation.Create("agent3");

        await _sut.SaveConversationAsync(conv1);
        await _sut.SaveConversationAsync(conv2);
        await _sut.SaveConversationAsync(conv3);

        // Act
        var list = await _sut.ListConversationsAsync();

        // Assert
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task ListConversationsAsync_ReturnsEmptyWhenNoConversations()
    {
        // Act
        var list = await _sut.ListConversationsAsync();

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public async Task ListConversationsAsync_OrdersByLastModifiedDescending()
    {
        // Arrange
        var conv1 = Conversation.Create("agent1");
        await _sut.SaveConversationAsync(conv1);

        await Task.Delay(10); // Ensure different timestamps

        var conv2 = Conversation.Create("agent2");
        await _sut.SaveConversationAsync(conv2);

        // Act
        var list = await _sut.ListConversationsAsync();

        // Assert
        Assert.Equal(conv2.Id, list[0].Id); // Most recent first
        Assert.Equal(conv1.Id, list[1].Id);
    }

    [Fact]
    public async Task DeleteConversationAsync_DeletesConversation()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent");
        await _sut.SaveConversationAsync(conversation);
        Assert.Equal(1, _sut.Count);

        // Act
        await _sut.DeleteConversationAsync(conversation.Id);

        // Assert
        Assert.Equal(0, _sut.Count);
        var loaded = await _sut.LoadConversationAsync(conversation.Id);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task DeleteConversationAsync_DoesNotThrowWhenNotFound()
    {
        // Act & Assert - Should not throw
        await _sut.DeleteConversationAsync("non-existent");
    }

    [Fact]
    public async Task DeleteConversationAsync_ThrowsOnNullOrWhitespace()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.DeleteConversationAsync(null!));
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.DeleteConversationAsync(""));
    }

    [Fact]
    public async Task Clear_RemovesAllConversations()
    {
        // Arrange
        await _sut.SaveConversationAsync(Conversation.Create("agent1"));
        await _sut.SaveConversationAsync(Conversation.Create("agent2"));

        // Act
        _sut.Clear();

        // Assert
        Assert.Equal(0, _sut.Count);
    }

    [Fact]
    public async Task SaveConversationAsync_UpdatesExistingConversation()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent");
        await _sut.SaveConversationAsync(conversation);

        // Act - Add a message and save again
        var updatedConversation = conversation.WithMessage(
            new ChatMessage(ChatRole.User, "Hello"));
        await _sut.SaveConversationAsync(updatedConversation);

        // Assert
        Assert.Equal(1, _sut.Count);
        var loaded = await _sut.LoadConversationAsync(conversation.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.Messages);
    }

    [Fact]
    public async Task ConversationMetadata_ContainsCorrectData()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent")
            .WithMessage(new ChatMessage(ChatRole.User, "Hello"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "Hi there!"));
        await _sut.SaveConversationAsync(conversation);

        // Act
        var list = await _sut.ListConversationsAsync();

        // Assert
        Assert.Single(list);
        var metadata = list[0];
        Assert.Equal(conversation.Id, metadata.Id);
        Assert.Equal("test-agent", metadata.AgentName);
        Assert.Equal(2, metadata.MessageCount);
    }

    [Fact]
    public async Task ConcurrentOperations_AreThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var agentName = $"agent-{i}";
            tasks.Add(Task.Run(async () =>
            {
                var conv = Conversation.Create(agentName);
                await _sut.SaveConversationAsync(conv);
                await _sut.LoadConversationAsync(conv.Id);
            }));
        }

        // Act & Assert - Should not throw
        await Task.WhenAll(tasks);
        Assert.Equal(100, _sut.Count);
    }
}
