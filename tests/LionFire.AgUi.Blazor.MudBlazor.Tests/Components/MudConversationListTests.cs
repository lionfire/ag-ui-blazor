using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudConversationList component.
/// </summary>
public class MudConversationListTests : TestContext
{
    public MudConversationListTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Render MudPopoverProvider to support MudTooltip components
        RenderComponent<MudPopoverProvider>();
    }

    private static List<ConversationMetadata> CreateTestConversations(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => new ConversationMetadata(
                Id: $"conv-{i}",
                AgentName: "test-agent",
                Title: $"Conversation {i}",
                CreatedAt: DateTimeOffset.UtcNow.AddDays(-i),
                LastModifiedAt: DateTimeOffset.UtcNow.AddHours(-i),
                MessageCount: i * 5,
                Tags: new List<string>()))
            .ToList();
    }

    #region Rendering Tests

    [Fact]
    public void Component_Renders_Successfully()
    {
        // Act
        var cut = RenderComponent<MudConversationList>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-conversation-list").Should().NotBeNull();
    }

    [Fact]
    public void Component_Shows_EmptyMessage_WhenNoConversations()
    {
        // Act
        var cut = RenderComponent<MudConversationList>();

        // Assert
        cut.Find(".conversation-list-empty").Should().NotBeNull();
        cut.Markup.Should().Contain("No conversations yet");
    }

    [Fact]
    public void Component_Shows_CustomEmptyMessage()
    {
        // Arrange
        var customMessage = "Start chatting!";

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.EmptyMessage, customMessage));

        // Assert
        cut.Markup.Should().Contain(customMessage);
    }

    [Fact]
    public void Component_Renders_ConversationList_WhenConversationsProvided()
    {
        // Arrange
        var conversations = CreateTestConversations();

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, conversations));

        // Assert
        var listItems = cut.FindComponents<MudListItem<ConversationMetadata>>();
        listItems.Should().HaveCount(3);
    }

    [Fact]
    public void Component_Displays_ConversationTitle()
    {
        // Arrange
        var conversations = CreateTestConversations(1);

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, conversations));

        // Assert
        cut.Markup.Should().Contain("Conversation 1");
    }

    [Fact]
    public void Component_Displays_MessageCount()
    {
        // Arrange
        var conversations = CreateTestConversations(1);

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, conversations));

        // Assert
        cut.Markup.Should().Contain("5 messages");
    }

    #endregion

    #region Title Generation Tests

    [Fact]
    public void Component_Displays_DefaultTitle_WhenTitleIsNull()
    {
        // Arrange
        var conversation = new ConversationMetadata(
            Id: "conv-1",
            AgentName: "my-agent",
            Title: null,
            CreatedAt: DateTimeOffset.UtcNow,
            LastModifiedAt: DateTimeOffset.UtcNow,
            MessageCount: 5,
            Tags: new List<string>());

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, new List<ConversationMetadata> { conversation }));

        // Assert
        cut.Markup.Should().Contain("Chat with my-agent");
    }

    #endregion

    #region Selection Tests

    [Fact]
    public void Component_Highlights_SelectedConversation()
    {
        // Arrange
        var conversations = CreateTestConversations();

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, conversations)
            .Add(p => p.SelectedConversationId, "conv-2"));

        // Assert
        cut.Markup.Should().Contain("conversation-item-selected");
    }

    [Fact]
    public async Task Component_Invokes_OnConversationSelected_WhenItemClicked()
    {
        // Arrange
        var conversations = CreateTestConversations();
        ConversationMetadata? selectedConversation = null;

        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, conversations)
            .Add(p => p.OnConversationSelected, (ConversationMetadata c) => selectedConversation = c));

        // Act - Click on list item using OnClick event
        var listItems = cut.FindComponents<MudListItem<ConversationMetadata>>();
        await cut.InvokeAsync(() => listItems[1].Instance.OnClick.InvokeAsync(null));

        // Assert
        selectedConversation.Should().NotBeNull();
        selectedConversation!.Id.Should().Be("conv-2");
    }

    #endregion

    #region New Conversation Tests

    [Fact]
    public void Component_Has_NewConversationButton()
    {
        // Act
        var cut = RenderComponent<MudConversationList>();

        // Assert
        var buttons = cut.FindComponents<MudIconButton>();
        buttons.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Component_Invokes_OnNewConversation_WhenButtonClicked()
    {
        // Arrange
        var newConversationClicked = false;

        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.OnNewConversation, () => newConversationClicked = true));

        // Act
        var addButton = cut.FindComponents<MudIconButton>().First();
        await cut.InvokeAsync(() => addButton.Instance.OnClick.InvokeAsync());

        // Assert
        newConversationClicked.Should().BeTrue();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Component_Shows_DeleteButton_ByDefault()
    {
        // Arrange
        var conversations = CreateTestConversations(1);

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, conversations));

        // Assert
        cut.Markup.Should().Contain("conversation-delete-btn");
    }

    [Fact]
    public void Component_Hides_DeleteButton_WhenDisabled()
    {
        // Arrange
        var conversations = CreateTestConversations(1);

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, conversations)
            .Add(p => p.ShowDeleteButton, false));

        // Assert
        cut.Markup.Should().NotContain("conversation-delete-btn");
    }

    #endregion

    #region Date Formatting Tests

    [Fact]
    public void Component_Formats_RecentDate_AsMinutesAgo()
    {
        // Arrange
        var conversation = new ConversationMetadata(
            Id: "conv-1",
            AgentName: "test-agent",
            Title: "Test",
            CreatedAt: DateTimeOffset.UtcNow,
            LastModifiedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            MessageCount: 1,
            Tags: new List<string>());

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, new List<ConversationMetadata> { conversation }));

        // Assert
        cut.Markup.Should().Contain("5 min ago");
    }

    [Fact]
    public void Component_Formats_VeryRecentDate_AsJustNow()
    {
        // Arrange
        var conversation = new ConversationMetadata(
            Id: "conv-1",
            AgentName: "test-agent",
            Title: "Test",
            CreatedAt: DateTimeOffset.UtcNow,
            LastModifiedAt: DateTimeOffset.UtcNow,
            MessageCount: 1,
            Tags: new List<string>());

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Conversations, new List<ConversationMetadata> { conversation }));

        // Assert
        cut.Markup.Should().Contain("Just now");
    }

    #endregion

    #region CSS Class Tests

    [Fact]
    public void Component_Applies_CustomClass()
    {
        // Arrange
        var customClass = "my-custom-class";

        // Act
        var cut = RenderComponent<MudConversationList>(parameters => parameters
            .Add(p => p.Class, customClass));

        // Assert
        cut.Find(".mud-conversation-list").ClassList.Should().Contain(customClass);
    }

    #endregion
}
