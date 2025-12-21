using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.JSInterop;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A component for displaying a list of chat messages with MudBlazor styling.
/// Supports auto-scrolling, streaming indicators, and role-based message styling.
/// </summary>
public partial class MudMessageList : ComponentBase, IAsyncDisposable
{
    private ElementReference _scrollContainer;
    private IJSObjectReference? _scrollModule;
    private bool _userHasScrolledUp;
    private int _previousMessageCount;
    private string? _previousLastMessageContent;
    private DotNetObjectReference<MudMessageList>? _dotNetRef;
    private int? _editingMessageIndex;
    private string? _editingContent;

    /// <summary>
    /// Gets or sets whether dark mode is active. When set, adjusts user bubble background color.
    /// Can be set via CascadingParameter from a parent component or explicitly via parameter.
    /// </summary>
    [CascadingParameter(Name = "IsDarkMode")]
    public bool? CascadedIsDarkMode { get; set; }

    /// <summary>
    /// Explicitly sets dark mode. Takes precedence over CascadingParameter.
    /// </summary>
    [Parameter]
    public bool? IsDarkMode { get; set; }

    private bool EffectiveIsDarkMode => IsDarkMode ?? CascadedIsDarkMode ?? false;

    /// <summary>
    /// MudBlazor constants for use in razor template.
    /// </summary>
    protected static string AssistantAvatarIcon => Icons.Material.Filled.SmartToy;
    protected static string UserAvatarIcon => Icons.Material.Filled.Person;
    protected static Size SmallSize => Size.Small;
    protected static Typo Body2Typo => Typo.body2;
    protected static Color TertiaryColor => Color.Tertiary;
    protected static string IconRefresh => Icons.Material.Filled.Refresh;
    protected static string IconEdit => Icons.Material.Filled.Edit;
    protected static string IconCopy => Icons.Material.Filled.ContentCopy;
    protected static string IconCheck => Icons.Material.Filled.Check;
    protected static string IconClose => Icons.Material.Filled.Close;
    protected static string IconStop => Icons.Material.Filled.Stop;

    // Variant constants for razor template
    protected static global::MudBlazor.Variant VariantOutlined => global::MudBlazor.Variant.Outlined;
    protected static global::MudBlazor.Variant VariantFilled => global::MudBlazor.Variant.Filled;
    protected static global::MudBlazor.Variant VariantText => global::MudBlazor.Variant.Text;

    // Color constants for razor template
    protected static Color PrimaryColor => Color.Primary;
    protected static Color ErrorColor => Color.Error;

    /// <summary>
    /// Gets or sets the list of messages to display.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ChatMessage>? Messages { get; set; }

    /// <summary>
    /// Gets or sets whether the assistant is currently streaming a response.
    /// When true, shows a typing indicator if the last message is from the assistant.
    /// </summary>
    [Parameter]
    public bool IsStreaming { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the user requests to regenerate a response.
    /// The int parameter is the index of the message to regenerate from.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnRegenerate { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the user edits a message.
    /// </summary>
    [Parameter]
    public EventCallback<MessageOperationEventArgs> OnEdit { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the user requests to stop generation.
    /// </summary>
    [Parameter]
    public EventCallback OnStop { get; set; }

    /// <summary>
    /// Gets or sets whether to show message action buttons (edit, regenerate, copy).
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowMessageActions { get; set; } = true;

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

    /// <summary>
    /// Gets or sets the message to display when there are no messages.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No messages yet. Start a conversation!";

    /// <summary>
    /// Gets or sets when to display sender titles (e.g., "You", "Assistant") in message headers.
    /// Default is Auto, which shows titles for non-user senders only when multiple distinct
    /// non-user senders are detected (useful for group chats).
    /// </summary>
    [Parameter]
    public SenderTitleDisplayMode SenderTitleDisplay { get; set; } = SenderTitleDisplayMode.Auto;

    /// <summary>
    /// Gets or sets the alignment of the current user's messages.
    /// Default is Right, which aligns user messages to the right side.
    /// When set to Left, user messages appear on the same side as other participants.
    /// </summary>
    [Parameter]
    public OwnMessagesAlignment OwnMessagesAlignment { get; set; } = OwnMessagesAlignment.Right;

    /// <summary>
    /// Gets or sets the display label for the current user's messages.
    /// Default is "You".
    /// </summary>
    [Parameter]
    public string UserLabel { get; set; } = "You";

    /// <summary>
    /// Gets or sets whether auto-scroll to bottom is enabled when new messages arrive.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool AutoScrollEnabled { get; set; } = true;

    /// <summary>
    /// The injected JavaScript runtime for interop.
    /// </summary>
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await InitializeScrollInteropAsync();
            // Initial scroll to bottom
            await ScrollToBottomAsync();
        }

        if (AutoScrollEnabled && Messages is not null)
        {
            var currentCount = Messages.Count;
            var currentLastContent = currentCount > 0 ? Messages[^1].Text : null;
            var messageCountChanged = currentCount > _previousMessageCount;
            var lastMessageContentChanged = IsStreaming &&
                currentLastContent?.Length != _previousLastMessageContent?.Length;

            // Always update tracking variables
            _previousMessageCount = currentCount;
            _previousLastMessageContent = currentLastContent;

            // Auto-scroll when new messages are added OR when streaming content is updated
            // But only if user hasn't scrolled up
            if (!_userHasScrolledUp && (messageCountChanged || lastMessageContentChanged))
            {
                await ScrollToBottomAsync();
            }
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // Reset scroll state when starting a new conversation
        var currentCount = Messages?.Count ?? 0;
        if (currentCount == 0 || (_previousMessageCount == 0 && currentCount > 0))
        {
            _userHasScrolledUp = false;
            _previousLastMessageContent = null;
            _previousMessageCount = 0;
        }
    }

    /// <summary>
    /// Determines if a message is from the user.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the message is from the user, false otherwise.</returns>
    protected bool IsUserMessage(ChatMessage message)
    {
        return message.Role == ChatRole.User;
    }

    /// <summary>
    /// Determines if the last message in the list is from the assistant.
    /// </summary>
    /// <returns>True if the last message is from the assistant, false otherwise.</returns>
    protected bool IsLastMessageFromAssistant()
    {
        if (Messages is null || Messages.Count == 0)
        {
            return false;
        }

        var lastMessage = Messages[Messages.Count - 1];
        return lastMessage.Role == ChatRole.Assistant;
    }

    /// <summary>
    /// Gets the CSS class for the message container based on the message role and alignment settings.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The CSS class string.</returns>
    protected string GetMessageContainerClass(ChatMessage message)
    {
        if (IsUserMessage(message))
        {
            return OwnMessagesAlignment == OwnMessagesAlignment.Left
                ? "message-container user-message user-message-left"
                : "message-container user-message";
        }
        return "message-container assistant-message";
    }

    /// <summary>
    /// Gets the CSS class for the avatar based on the message role.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The CSS class string.</returns>
    protected string GetAvatarClass(ChatMessage message)
    {
        return IsUserMessage(message)
            ? "message-avatar user-avatar"
            : "message-avatar assistant-avatar";
    }

    /// <summary>
    /// Gets the icon for the avatar based on the message role.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The icon string.</returns>
    protected string GetAvatarIcon(ChatMessage message)
    {
        return IsUserMessage(message) ? UserAvatarIcon : AssistantAvatarIcon;
    }

    /// <summary>
    /// Gets the CSS class for the message bubble based on the message role.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The CSS class string.</returns>
    protected string GetMessageBubbleClass(ChatMessage message)
    {
        return IsUserMessage(message)
            ? "message-bubble user-bubble"
            : "message-bubble assistant-bubble";
    }

    /// <summary>
    /// Gets the inline style for the message bubble based on the message role and theme.
    /// Uses inline styles to ensure proper colors in both light and dark mode.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The inline style string.</returns>
    protected string GetMessageBubbleStyle(ChatMessage message)
    {
        if (IsUserMessage(message))
        {
            // In dark mode, use a darker background for user bubbles for better contrast
            var bgColor = EffectiveIsDarkMode
                ? "var(--mud-palette-gray-darker)"
                : "var(--mud-palette-gray-lighter)";
            return $"background-color: {bgColor}; color: var(--mud-palette-text-primary);";
        }
        else
        {
            return "background-color: var(--mud-palette-background-gray); color: var(--mud-palette-text-primary);";
        }
    }

    /// <summary>
    /// Gets the display name for a message role.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The human-readable role name.</returns>
    protected string GetRoleDisplayName(ChatMessage message)
    {
        return message.Role.Value switch
        {
            "user" => UserLabel,
            "assistant" => "Assistant",
            "system" => "System",
            "tool" => "Tool",
            _ => message.Role.Value
        };
    }

    /// <summary>
    /// Determines whether the sender title should be displayed for a given message
    /// based on the current <see cref="SenderTitleDisplay"/> setting.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the sender title should be displayed; otherwise, false.</returns>
    protected bool ShouldShowSenderTitle(ChatMessage message)
    {
        return SenderTitleDisplay switch
        {
            SenderTitleDisplayMode.Enabled => true,
            SenderTitleDisplayMode.Disabled => false,
            SenderTitleDisplayMode.Auto => ShouldShowSenderTitleAuto(message),
            _ => true
        };
    }

    /// <summary>
    /// Auto-detection logic for sender title visibility.
    /// Shows titles for non-user senders only when multiple distinct non-user senders exist.
    /// For user messages, only shows title when OwnMessagesAlignment is Left.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the sender title should be displayed in Auto mode.</returns>
    private bool ShouldShowSenderTitleAuto(ChatMessage message)
    {
        // For user messages, only show label when messages are left-aligned
        // (when right-aligned, visual position already distinguishes them)
        if (IsUserMessage(message))
        {
            return OwnMessagesAlignment == OwnMessagesAlignment.Left;
        }

        // For non-user messages, check if there are multiple distinct non-user senders
        if (Messages == null || Messages.Count == 0)
        {
            return true;
        }

        // Count distinct non-user roles
        var distinctNonUserRoles = Messages
            .Where(m => !IsUserMessage(m))
            .Select(m => m.Role.Value)
            .Distinct()
            .Count();

        // Show title only if there are multiple distinct non-user senders
        return distinctNonUserRoles > 1;
    }

    /// <summary>
    /// Formats the timestamp for display.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>A formatted timestamp string.</returns>
    protected string FormatTimestamp(ChatMessage message)
    {
        // ChatMessage from Microsoft.Extensions.AI doesn't have a built-in timestamp.
        // For now, we return an empty string. In a real implementation, you would
        // either extend the message type or store timestamps separately.
        // This is a placeholder for future enhancement.
        return string.Empty;
    }

    /// <summary>
    /// Extracts the text content from a chat message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The text content of the message.</returns>
    protected string GetMessageContent(ChatMessage message)
    {
        // ChatMessage.Text property provides the concatenated text content
        // from all TextContent items in the Contents collection
        return message.Text ?? string.Empty;
    }

    /// <summary>
    /// Scrolls the message list to the bottom.
    /// </summary>
    public async Task ScrollToBottomAsync()
    {
        try
        {
            Console.WriteLine($"ScrollToBottomAsync called, _scrollModule is {(_scrollModule is null ? "null" : "set")}");
            if (_scrollModule is not null)
            {
                await _scrollModule.InvokeVoidAsync("scrollToBottom", _scrollContainer);
            }
            else
            {
                // Fallback to inline JS if module not loaded
                Console.WriteLine("Using fallback scroll");
                await JSRuntime.InvokeVoidAsync("eval",
                    "arguments[0].scrollTop = arguments[0].scrollHeight", _scrollContainer);
            }
        }
        catch (JSException ex)
        {
            Console.WriteLine($"ScrollToBottomAsync error: {ex.Message}");
        }
    }

    /// <summary>
    /// Called from JavaScript when the user scrolls the message list.
    /// </summary>
    /// <param name="isAtBottom">Whether the scroll position is at the bottom.</param>
    [JSInvokable]
    public void OnScroll(bool isAtBottom)
    {
        _userHasScrolledUp = !isAtBottom;
    }

    /// <summary>
    /// Resets the scroll state, allowing auto-scroll to resume.
    /// </summary>
    public void ResetScrollState()
    {
        _userHasScrolledUp = false;
    }

    private async Task InitializeScrollInteropAsync()
    {
        try
        {
            _scrollModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/LionFire.AgUi.Blazor.MudBlazor/js/message-list-interop.js"
            );

            if (_scrollModule is not null && _dotNetRef is not null)
            {
                await _scrollModule.InvokeVoidAsync("initScrollListener", _scrollContainer, _dotNetRef);
            }
        }
        catch (JSException)
        {
            // Module not available, will use fallback scrolling
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_scrollModule is not null)
        {
            try
            {
                await _scrollModule.InvokeVoidAsync("removeScrollListener", _scrollContainer);
                await _scrollModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit is disconnected, ignore
            }
        }

        _dotNetRef?.Dispose();
    }

    #region Message Actions

    /// <summary>
    /// Determines if this is the last assistant message (for regenerate button).
    /// </summary>
    /// <param name="index">The message index.</param>
    /// <returns>True if this is the last assistant message.</returns>
    protected bool IsLastAssistantMessage(int index)
    {
        if (Messages is null || index < 0 || index >= Messages.Count)
            return false;

        // Check if this is an assistant message and there are no assistant messages after it
        if (Messages[index].Role != ChatRole.Assistant)
            return false;

        for (int i = index + 1; i < Messages.Count; i++)
        {
            if (Messages[i].Role == ChatRole.Assistant)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Starts editing a message.
    /// </summary>
    /// <param name="index">The index of the message to edit.</param>
    protected void StartEditing(int index)
    {
        if (Messages is null || index < 0 || index >= Messages.Count)
            return;

        _editingMessageIndex = index;
        _editingContent = GetMessageContent(Messages[index]);
    }

    /// <summary>
    /// Cancels the current edit operation.
    /// </summary>
    protected void CancelEditing()
    {
        _editingMessageIndex = null;
        _editingContent = null;
    }

    /// <summary>
    /// Saves the edited message content.
    /// </summary>
    protected async Task SaveEditAsync()
    {
        if (_editingMessageIndex.HasValue && !string.IsNullOrWhiteSpace(_editingContent))
        {
            var args = new MessageOperationEventArgs(
                _editingMessageIndex.Value,
                MessageOperationType.Edit,
                _editingContent);

            if (OnEdit.HasDelegate)
            {
                await OnEdit.InvokeAsync(args);
            }
        }

        CancelEditing();
    }

    /// <summary>
    /// Handles the regenerate action.
    /// </summary>
    /// <param name="index">The index of the message to regenerate from.</param>
    protected async Task HandleRegenerateAsync(int index)
    {
        if (OnRegenerate.HasDelegate)
        {
            await OnRegenerate.InvokeAsync(index);
        }
    }

    /// <summary>
    /// Handles the stop action.
    /// </summary>
    protected async Task HandleStopAsync()
    {
        if (OnStop.HasDelegate)
        {
            await OnStop.InvokeAsync();
        }
    }

    /// <summary>
    /// Copies message content to clipboard.
    /// </summary>
    /// <param name="index">The index of the message to copy.</param>
    protected async Task CopyToClipboardAsync(int index)
    {
        if (Messages is null || index < 0 || index >= Messages.Count)
            return;

        var content = GetMessageContent(Messages[index]);
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", content);
        }
        catch (JSException)
        {
            // Clipboard API may not be available
        }
    }

    /// <summary>
    /// Gets whether a specific message is currently being edited.
    /// </summary>
    /// <param name="index">The message index.</param>
    /// <returns>True if the message is being edited.</returns>
    protected bool IsEditing(int index) => _editingMessageIndex == index;

    #endregion
}
