using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A complete chat interface with conversation history sidebar.
/// Combines MudAgentChat with MudConversationList in a responsive layout.
/// </summary>
public partial class MudAgentChatWithHistory : ComponentBase
{
    #region MudBlazor Constants

    protected static string IconMenu => Icons.Material.Filled.Menu;
    protected static Color InheritColor => Color.Inherit;
    protected static Edge EdgeStart => Edge.Start;
    protected static Typo H6Typo => Typo.h6;
    protected static Typo Body1Typo => Typo.body1;
    protected static Color TertiaryColor => Color.Tertiary;

    #endregion

    #region State

    private bool _drawerOpen = true;
    private MudAgentChat? _chatComponent;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the list of conversations to display in the sidebar.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ConversationMetadata>? Conversations { get; set; }

    /// <summary>
    /// Gets or sets the ID of the currently selected conversation.
    /// </summary>
    [Parameter]
    public string? SelectedConversationId { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected conversation ID changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> SelectedConversationIdChanged { get; set; }

    /// <summary>
    /// Gets or sets the name of the agent to chat with.
    /// </summary>
    [Parameter]
    public string? AgentName { get; set; }

    /// <summary>
    /// Gets or sets whether to show the connection status indicator.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowConnectionStatus { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show delete buttons on conversations.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowDeleteButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback invoked when a conversation is selected.
    /// </summary>
    [Parameter]
    public EventCallback<ConversationMetadata> OnConversationSelected { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a new conversation is requested.
    /// </summary>
    [Parameter]
    public EventCallback OnNewConversation { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a conversation is deleted.
    /// </summary>
    [Parameter]
    public EventCallback<ConversationMetadata> OnConversationDeleted { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a message is sent.
    /// </summary>
    [Parameter]
    public EventCallback<ChatMessage> OnMessageSent { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a message is received.
    /// </summary>
    [Parameter]
    public EventCallback<ChatMessage> OnMessageReceived { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the layout.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the layout.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current chat component instance.
    /// </summary>
    public MudAgentChat? ChatComponent => _chatComponent;

    /// <summary>
    /// Toggles the conversation drawer open/closed.
    /// </summary>
    public void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    #endregion

    #region Helper Methods

    private string GetLayoutClass()
    {
        return string.IsNullOrWhiteSpace(Class)
            ? "mud-agent-chat-with-history"
            : $"mud-agent-chat-with-history {Class}";
    }

    private string GetDrawerClass()
    {
        return _drawerOpen
            ? "conversation-drawer drawer-open"
            : "conversation-drawer";
    }

    private string GetChatTitle()
    {
        if (string.IsNullOrEmpty(SelectedConversationId) || Conversations is null)
        {
            return $"Chat with {AgentName}";
        }

        var conversation = Conversations.FirstOrDefault(c => c.Id == SelectedConversationId);
        if (conversation is not null && !string.IsNullOrWhiteSpace(conversation.Title))
        {
            return conversation.Title;
        }

        return $"Chat with {AgentName}";
    }

    #endregion

    #region Event Handlers

    private async Task HandleConversationSelectedAsync(ConversationMetadata conversation)
    {
        SelectedConversationId = conversation.Id;

        if (SelectedConversationIdChanged.HasDelegate)
        {
            await SelectedConversationIdChanged.InvokeAsync(conversation.Id);
        }

        if (OnConversationSelected.HasDelegate)
        {
            await OnConversationSelected.InvokeAsync(conversation);
        }
    }

    private async Task HandleNewConversationAsync()
    {
        if (OnNewConversation.HasDelegate)
        {
            await OnNewConversation.InvokeAsync();
        }
    }

    private async Task HandleDeleteConversationAsync(ConversationMetadata conversation)
    {
        if (OnConversationDeleted.HasDelegate)
        {
            await OnConversationDeleted.InvokeAsync(conversation);
        }
    }

    private async Task HandleMessageSentAsync(ChatMessage message)
    {
        if (OnMessageSent.HasDelegate)
        {
            await OnMessageSent.InvokeAsync(message);
        }
    }

    private async Task HandleMessageReceivedAsync(ChatMessage message)
    {
        if (OnMessageReceived.HasDelegate)
        {
            await OnMessageReceived.InvokeAsync(message);
        }
    }

    #endregion
}
