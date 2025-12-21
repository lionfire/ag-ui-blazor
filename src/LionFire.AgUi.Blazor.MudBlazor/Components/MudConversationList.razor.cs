using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A component for displaying a list of conversations with selection and management capabilities.
/// </summary>
public partial class MudConversationList : ComponentBase
{
    #region MudBlazor Constants

    /// <summary>
    /// MudBlazor constants for use in razor template.
    /// </summary>
    protected static string IconAdd => Icons.Material.Filled.Add;
    protected static string IconDelete => Icons.Material.Filled.Delete;
    protected static Size SmallSize => Size.Small;
    protected static Size ExtraSmallSize => Size.Small;
    protected static Typo H6Typo => Typo.h6;
    protected static Typo Body2Typo => Typo.body2;
    protected static Typo Subtitle2Typo => Typo.subtitle2;
    protected static Typo CaptionTypo => Typo.caption;
    protected static Color TertiaryColor => Color.Tertiary;
    protected static Color PrimaryColor => Color.Primary;
    protected static Color ErrorColor => Color.Error;
    protected static global::MudBlazor.Variant ChipVariant => global::MudBlazor.Variant.Outlined;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the list of conversation metadata to display.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ConversationMetadata>? Conversations { get; set; }

    /// <summary>
    /// Gets or sets the ID of the currently selected conversation.
    /// </summary>
    [Parameter]
    public string? SelectedConversationId { get; set; }

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
    public EventCallback<ConversationMetadata> OnDelete { get; set; }

    /// <summary>
    /// Gets or sets whether to show the delete button for each conversation.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowDeleteButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the message displayed when there are no conversations.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No conversations yet. Start a new one!";

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the title to display for a conversation.
    /// Auto-generates from first user message if no title is set.
    /// </summary>
    /// <param name="conversation">The conversation metadata.</param>
    /// <returns>The title to display.</returns>
    protected string GetConversationTitle(ConversationMetadata conversation)
    {
        if (!string.IsNullOrWhiteSpace(conversation.Title))
        {
            return conversation.Title;
        }

        // Default title based on agent name and date
        return $"Chat with {conversation.AgentName}";
    }

    /// <summary>
    /// Formats the date for display.
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>A formatted date string.</returns>
    protected string FormatDate(DateTimeOffset date)
    {
        var now = DateTimeOffset.UtcNow;
        var diff = now - date;

        if (diff.TotalMinutes < 1)
        {
            return "Just now";
        }

        if (diff.TotalHours < 1)
        {
            var minutes = (int)diff.TotalMinutes;
            return $"{minutes} min ago";
        }

        if (diff.TotalDays < 1)
        {
            var hours = (int)diff.TotalHours;
            return $"{hours}h ago";
        }

        if (diff.TotalDays < 7)
        {
            var days = (int)diff.TotalDays;
            return $"{days}d ago";
        }

        return date.ToString("MMM d, yyyy");
    }

    /// <summary>
    /// Gets the CSS class for a conversation item based on selection state.
    /// </summary>
    /// <param name="conversation">The conversation metadata.</param>
    /// <returns>The CSS class string.</returns>
    protected string GetConversationItemClass(ConversationMetadata conversation)
    {
        return conversation.Id == SelectedConversationId
            ? "conversation-item-selected"
            : string.Empty;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the new conversation button click.
    /// </summary>
    private async Task HandleNewConversationAsync()
    {
        if (OnNewConversation.HasDelegate)
        {
            await OnNewConversation.InvokeAsync();
        }
    }

    /// <summary>
    /// Handles a conversation being selected.
    /// </summary>
    /// <param name="conversation">The selected conversation.</param>
    private async Task HandleConversationSelectedAsync(ConversationMetadata conversation)
    {
        if (OnConversationSelected.HasDelegate)
        {
            await OnConversationSelected.InvokeAsync(conversation);
        }
    }

    /// <summary>
    /// Handles the delete button click for a conversation.
    /// </summary>
    /// <param name="conversation">The conversation to delete.</param>
    private async Task HandleDeleteAsync(ConversationMetadata conversation)
    {
        if (OnDelete.HasDelegate)
        {
            await OnDelete.InvokeAsync(conversation);
        }
    }

    #endregion
}
