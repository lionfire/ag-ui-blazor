using System.Net.Http;
using System.Text;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// Main chat component that orchestrates the chat UI, manages conversation state,
/// handles streaming responses, and coordinates child components.
/// </summary>
/// <remarks>
/// <para>
/// This component provides a complete chat interface for interacting with AI agents.
/// It uses the <see cref="IAgentClientFactory"/> to obtain agent instances and
/// <see cref="IChatClient"/> for streaming responses.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// <list type="bullet">
/// <item>Real-time streaming of agent responses</item>
/// <item>Connection status indicator</item>
/// <item>Error display with retry support</item>
/// <item>Full MudBlazor theming support</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;MudAgentChat AgentName="assistant"
///               ShowConnectionStatus="true"
///               OnMessageSent="HandleMessageSent"
///               OnMessageReceived="HandleMessageReceived" /&gt;
/// </code>
/// </example>
public partial class MudAgentChat : ComponentBase, IDisposable
{
    #region Constants

    /// <summary>
    /// MudBlazor Severity.Error constant for use in razor template.
    /// </summary>
    protected static Severity SeverityError => Severity.Error;

    /// <summary>
    /// MudBlazor Variant.Outlined for alerts.
    /// </summary>
    protected static Variant AlertVariant => Variant.Outlined;

    /// <summary>
    /// Icon for queued messages indicator.
    /// </summary>
    protected static string QueueIcon => Icons.Material.Filled.Queue;

    /// <summary>
    /// Info color for chip.
    /// </summary>
    protected static Color InfoColor => Color.Info;

    /// <summary>
    /// Small size for chip.
    /// </summary>
    protected static Size SmallSize => Size.Small;

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the name of the agent to chat with.
    /// This parameter is required.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to show the connection status indicator.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowConnectionStatus { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback invoked when a message is sent by the user.
    /// </summary>
    [Parameter]
    public EventCallback<ChatMessage> OnMessageSent { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a message is received from the agent.
    /// </summary>
    [Parameter]
    public EventCallback<ChatMessage> OnMessageReceived { get; set; }

    /// <summary>
    /// Gets or sets initial messages to seed the conversation with (e.g., when loading a saved conversation).
    /// </summary>
    [Parameter]
    public IReadOnlyList<ChatMessage>? InitialMessages { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the user requests a new chat (Ctrl+K).
    /// If not set, the conversation will be cleared instead.
    /// </summary>
    [Parameter]
    public EventCallback OnNewChat { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets the thinking/reasoning display mode.
    /// Default is Hide, which merges thinking into the regular response.
    /// </summary>
    [Parameter]
    public ThinkingDisplayMode ThinkingDisplay { get; set; } = ThinkingDisplayMode.Hide;

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Optional content rendered at the left of the composer footer row (beneath the
    /// text field). Forwarded to <see cref="MudMessageInput.FooterStart"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ComposerFooterStart { get; set; }

    /// <summary>
    /// Optional content rendered right-aligned in the composer footer row, before the
    /// send button. Forwarded to <see cref="MudMessageInput.FooterEnd"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ComposerFooterEnd { get; set; }

    /// <summary>
    /// Optional template rendered beneath each message bubble (e.g., model attribution).
    /// The template receives the <see cref="ChatMessage"/> and decides whether to render
    /// anything. Forwarded to <see cref="MudMessageList.MessageFooter"/>.
    /// </summary>
    [Parameter]
    public RenderFragment<ChatMessage>? MessageFooter { get; set; }

    /// <summary>
    /// Optional provider of per-request <see cref="ChatOptions"/>, evaluated at send time
    /// for each request (including regenerate/edit resends). Enables consumers to supply
    /// model overrides, sampling parameters, or tools without re-creating the component.
    /// Return null for provider defaults.
    /// </summary>
    [Parameter]
    public Func<ChatOptions?>? ChatOptionsProvider { get; set; }

    #endregion

    #region Injected Services

    /// <summary>
    /// Factory for obtaining agent instances.
    /// </summary>
    [Inject]
    private IAgentClientFactory AgentFactory { get; set; } = null!;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    [Inject]
    private ILogger<MudAgentChat> Logger { get; set; } = null!;

    /// <summary>
    /// Service provider for optional service resolution.
    /// </summary>
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    /// <summary>
    /// MudBlazor dialog service for showing dialogs.
    /// </summary>
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    #endregion

    #region State

    /// <summary>
    /// Keyboard shortcut service for handling shortcuts.
    /// Resolved optionally at runtime - may be null if not registered.
    /// </summary>
    private IKeyboardShortcutService? _keyboardShortcutService;

    /// <summary>
    /// The list of messages in the conversation.
    /// </summary>
    private readonly List<ChatMessage> _messages = new();

    /// <summary>
    /// Queue of messages waiting to be sent after current streaming completes.
    /// </summary>
    private readonly Queue<PendingMessage> _messageQueue = new();

    /// <summary>
    /// Whether the agent is currently streaming a response.
    /// </summary>
    private bool _isStreaming;

    /// <summary>
    /// The current connection state.
    /// </summary>
    private ConnectionState _connectionState = ConnectionState.Disconnected;

    /// <summary>
    /// The current error message, if any.
    /// </summary>
    private string? _errorMessage;

    /// <summary>
    /// The current agent instance.
    /// </summary>
    private IChatClient? _agent;

    /// <summary>
    /// Cancellation token source for canceling streaming operations.
    /// </summary>
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Whether the component has been disposed.
    /// </summary>
    private bool _disposed;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the current list of messages in the conversation.
    /// Useful for testing and external access.
    /// </summary>
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Gets whether the agent is currently streaming a response.
    /// </summary>
    public bool IsStreaming => _isStreaming;

    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    public ConnectionState ConnectionState => _connectionState;

    /// <summary>
    /// Gets the current error message, if any.
    /// </summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>
    /// Gets the pending messages waiting to be sent.
    /// </summary>
    public IReadOnlyCollection<PendingMessage> PendingMessages => _messageQueue.ToArray();

    /// <summary>
    /// Gets the count of queued messages.
    /// </summary>
    public int QueuedMessageCount => _messageQueue.Count;

    #endregion

    #region Lifecycle

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        // Resolve optional services
        _keyboardShortcutService = ServiceProvider.GetService(typeof(IKeyboardShortcutService)) as IKeyboardShortcutService;

        if (string.IsNullOrWhiteSpace(AgentName))
        {
            throw new ArgumentException("AgentName is required", nameof(AgentName));
        }

        Logger.LogDebug("Initializing MudAgentChat for agent: {AgentName}", AgentName);

        try
        {
            _connectionState = ConnectionState.Connecting;
            StateHasChanged();

            _agent = await AgentFactory.GetAgentAsync(AgentName);

            if (_agent == null)
            {
                _errorMessage = $"Agent '{AgentName}' not found";
                _connectionState = ConnectionState.Error;
                Logger.LogWarning("Agent not found: {AgentName}", AgentName);
            }
            else
            {
                _connectionState = AgentFactory.GetConnectionState();
                Logger.LogInformation("Successfully initialized agent: {AgentName}", AgentName);
            }

            if (InitialMessages is { Count: > 0 })
            {
                _messages.AddRange(InitialMessages);
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to initialize agent: {ex.Message}";
            _connectionState = ConnectionState.Error;
            Logger.LogError(ex, "Failed to initialize agent {AgentName}", AgentName);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Logger.LogDebug("Disposing MudAgentChat for agent: {AgentName}", AgentName);

            // Cancel any ongoing streaming
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            // Dispose agent if it implements IDisposable
            if (_agent is IDisposable disposableAgent)
            {
                disposableAgent.Dispose();
            }

            _agent = null;
        }

        _disposed = true;
    }

    #endregion

    #region Message Handling

    /// <summary>
    /// Sends a user message and processes the agent's streaming response.
    /// If streaming is in progress, the message is queued for later delivery.
    /// </summary>
    /// <param name="messageText">The text of the message to send.</param>
    public async Task SendMessageAsync(string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return;
        }

        if (_agent == null)
        {
            _errorMessage = "Agent is not available. No commands are registered.";
            return;
        }

        // If currently streaming, queue the message for later
        if (_isStreaming)
        {
            Logger.LogDebug("Queueing message while streaming: {MessagePreview}",
                messageText.Length > 50 ? messageText[..50] + "..." : messageText);
            _messageQueue.Enqueue(new PendingMessage(messageText));
            StateHasChanged();
            return;
        }

        await ProcessAgentResponseAsync(messageText);
    }

    /// <summary>
    /// Processes the agent's streaming response for a user message.
    /// </summary>
    /// <param name="userMessage">The user's message text.</param>
    private async Task ProcessAgentResponseAsync(string userMessage)
    {
        // Cancel any previous streaming operation
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var cancellationToken = _cts.Token;

        try
        {
            _isStreaming = true;
            ClearError();

            // Add user message
            var userMsg = new ChatMessage(ChatRole.User, userMessage);
            _messages.Add(userMsg);

            if (OnMessageSent.HasDelegate)
            {
                await OnMessageSent.InvokeAsync(userMsg);
            }

            StateHasChanged();

            Logger.LogDebug("Sending message to agent {AgentName}: {MessagePreview}",
                AgentName, userMessage.Length > 50 ? userMessage[..50] + "..." : userMessage);

            // Snapshot the conversation to send BEFORE adding the visual placeholder:
            // a trailing empty assistant message reaches the provider otherwise, and
            // several chat templates interpret that as "the assistant already answered"
            // and immediately emit end-of-turn — observed live as instant empty
            // responses (finish_reason=stop, empty delta) from LM Studio.
            var requestMessages = _messages.ToList();

            // Create assistant message placeholder
            var assistantMsg = new ChatMessage(ChatRole.Assistant, string.Empty);
            _messages.Add(assistantMsg);
            StateHasChanged();

            // Evaluate per-request options at send time so the latest consumer-side
            // selections (model override, sampling parameters) apply to this request.
            ChatOptions? chatOptions = null;
            if (ChatOptionsProvider is not null)
            {
                try
                {
                    chatOptions = ChatOptionsProvider();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "ChatOptionsProvider threw; proceeding without options");
                }
            }

            // Stream response, separating reasoning from regular content
            var contentBuilder = new StringBuilder();
            var reasoningBuilder = new StringBuilder();
            string? responseModelId = null;
            await foreach (var update in _agent!.GetStreamingResponseAsync(requestMessages, chatOptions, cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.ModelId))
                {
                    responseModelId = update.ModelId;
                }

                if (update.Text != null)
                {
                    var isReasoning = update.AdditionalProperties?.TryGetValue("is_reasoning", out var val) == true
                                      && val is true;

                    if (isReasoning && ThinkingDisplay != ThinkingDisplayMode.Hide)
                    {
                        reasoningBuilder.Append(update.Text);
                    }
                    else
                    {
                        contentBuilder.Append(update.Text);
                    }

                    // Build message with separate content items when we have reasoning
                    if (reasoningBuilder.Length > 0 && ThinkingDisplay != ThinkingDisplayMode.Hide)
                    {
                        var contents = new List<AIContent>();
                        var reasoningContent = new TextContent(reasoningBuilder.ToString());
                        reasoningContent.AdditionalProperties ??= new AdditionalPropertiesDictionary();
                        reasoningContent.AdditionalProperties["is_reasoning"] = true;
                        contents.Add(reasoningContent);
                        contents.Add(new TextContent(contentBuilder.ToString()));
                        _messages[^1] = new ChatMessage(ChatRole.Assistant, contents);
                    }
                    else
                    {
                        _messages[^1] = new ChatMessage(ChatRole.Assistant, contentBuilder.ToString());
                    }
                    StateHasChanged();
                }
            }

            Logger.LogDebug("Received complete response from agent {AgentName}, length: {ResponseLength} (reasoning: {ReasoningLength})",
                AgentName, contentBuilder.Length, reasoningBuilder.Length);

            // A stream that completed without producing anything is a failure the user
            // must see — an empty bubble that looks like an answer is not an answer.
            if (contentBuilder.Length == 0 && reasoningBuilder.Length == 0)
            {
                RemoveTrailingEmptyAssistantPlaceholder();
                _errorMessage = "The model returned an empty response. It may have mishandled the request — try again, or try another model.";
                Logger.LogWarning("Agent {AgentName} completed streaming with an empty response", AgentName);
                return;
            }

            // Successful response — update connection state if it was degraded
            if (_connectionState != ConnectionState.Connected)
            {
                _connectionState = AgentFactory.GetConnectionState();
            }

            // Stamp the model that produced the response so consumers (attribution
            // captions, persistence) can read it from the message itself.
            if (responseModelId is not null
                && _messages.Count > 0
                && _messages[^1].Role == ChatRole.Assistant)
            {
                var finalMessage = _messages[^1];
                finalMessage.AdditionalProperties ??= new AdditionalPropertiesDictionary();
                finalMessage.AdditionalProperties["model_id"] = responseModelId;
            }

            if (OnMessageReceived.HasDelegate)
            {
                await OnMessageReceived.InvokeAsync(_messages[^1]);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("Streaming response cancelled for agent {AgentName}", AgentName);
            // User cancelled - this is expected, don't show error
            RemoveTrailingEmptyAssistantPlaceholder();
        }
        catch (OperationCanceledException ex)
        {
            // NOT user-initiated: an HttpClient/timeout cancellation (TaskCanceledException
            // derives from OperationCanceledException). Swallowing this as "user cancelled"
            // is how a timed-out send once died silently with no visible error.
            RemoveTrailingEmptyAssistantPlaceholder();
            _errorMessage = "The request timed out before the model responded. The endpoint may be down or overloaded.";
            Logger.LogError(ex, "Non-user cancellation (timeout) while communicating with agent {AgentName}", AgentName);
        }
        catch (HttpRequestException ex)
        {
            RemoveTrailingEmptyAssistantPlaceholder();
            _errorMessage = $"Network error: {ex.Message}";
            _connectionState = ConnectionState.Error;
            Logger.LogError(ex, "Network error while communicating with agent {AgentName}", AgentName);
        }
        catch (TimeoutException ex)
        {
            RemoveTrailingEmptyAssistantPlaceholder();
            _errorMessage = $"Request timed out: {ex.Message}";
            Logger.LogError(ex, "Timeout while communicating with agent {AgentName}", AgentName);
        }
        catch (Exception ex)
        {
            RemoveTrailingEmptyAssistantPlaceholder();
            _errorMessage = $"Error: {ex.Message}";
            Logger.LogError(ex, "Error processing agent response for {AgentName}", AgentName);
        }
        finally
        {
            _isStreaming = false;
            StateHasChanged();

            // Process any pending messages
            await ProcessPendingMessagesAsync();
        }
    }

    /// <summary>
    /// Removes the streaming placeholder when a send failed or produced nothing, so no
    /// empty assistant bubble lingers (or gets persisted) looking like an answer.
    /// Only a trailing assistant message with no content is ever removed.
    /// </summary>
    private void RemoveTrailingEmptyAssistantPlaceholder()
    {
        if (_messages.Count > 0
            && _messages[^1].Role == ChatRole.Assistant
            && string.IsNullOrEmpty(_messages[^1].Text))
        {
            _messages.RemoveAt(_messages.Count - 1);
        }
    }

    /// <summary>
    /// Processes any messages that were queued while streaming was in progress.
    /// </summary>
    private async Task ProcessPendingMessagesAsync()
    {
        if (_messageQueue.Count == 0)
        {
            return;
        }

        var pendingMessage = _messageQueue.Dequeue();
        Logger.LogDebug("Processing pending message: {MessagePreview}",
            pendingMessage.Content.Length > 50 ? pendingMessage.Content[..50] + "..." : pendingMessage.Content);

        // Process the pending message (this will trigger another streaming response)
        await ProcessAgentResponseAsync(pendingMessage.Content);
    }

    /// <summary>
    /// Cancels the current streaming operation.
    /// </summary>
    public void CancelStreaming()
    {
        if (_isStreaming && _cts != null)
        {
            Logger.LogDebug("Cancelling streaming for agent {AgentName}", AgentName);
            _cts.Cancel();
        }
    }

    /// <summary>
    /// Clears the current error message.
    /// </summary>
    public void ClearError()
    {
        _errorMessage = null;

        // If we had an error state, try to reconnect
        if (_connectionState == ConnectionState.Error && _agent != null)
        {
            _connectionState = AgentFactory.GetConnectionState();
        }
    }

    /// <summary>
    /// Clears all messages in the conversation and the message queue.
    /// </summary>
    public void ClearConversation()
    {
        _messages.Clear();
        _messageQueue.Clear();
        StateHasChanged();
    }

    /// <summary>
    /// Clears any queued messages without clearing the conversation.
    /// </summary>
    public void ClearQueue()
    {
        _messageQueue.Clear();
        StateHasChanged();
    }

    /// <summary>
    /// Handles the stop streaming request from MudMessageList.
    /// </summary>
    private Task HandleStopAsync()
    {
        CancelStreaming();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the regenerate request from MudMessageList.
    /// Removes messages from the given index and resends from the last user message.
    /// </summary>
    /// <param name="messageIndex">The index of the message to regenerate from.</param>
    private async Task HandleRegenerateAsync(int messageIndex)
    {
        if (_agent == null || _isStreaming)
        {
            return;
        }

        // Find the user message that triggered the assistant response we're regenerating
        if (messageIndex <= 0 || messageIndex >= _messages.Count)
        {
            return;
        }

        // The regenerate button is on an assistant message, find the preceding user message
        int userMessageIndex = messageIndex - 1;
        while (userMessageIndex >= 0 && _messages[userMessageIndex].Role != ChatRole.User)
        {
            userMessageIndex--;
        }

        if (userMessageIndex < 0)
        {
            Logger.LogWarning("Could not find user message before index {MessageIndex} for regeneration", messageIndex);
            return;
        }

        var userMessage = _messages[userMessageIndex].Text ?? string.Empty;

        // Remove the assistant message(s) from the regeneration point onwards
        while (_messages.Count > messageIndex)
        {
            _messages.RemoveAt(_messages.Count - 1);
        }

        // Also remove the user message that will be re-added by ProcessAgentResponseAsync
        _messages.RemoveAt(userMessageIndex);

        Logger.LogDebug("Regenerating response from message index {MessageIndex}, user message: {MessagePreview}",
            messageIndex, userMessage.Length > 50 ? userMessage[..50] + "..." : userMessage);

        StateHasChanged();

        // Resend with the same user message
        await ProcessAgentResponseAsync(userMessage);
    }

    /// <summary>
    /// Handles the edit request from MudMessageList.
    /// Truncates conversation and resends with the edited message.
    /// </summary>
    /// <param name="args">The message operation event arguments.</param>
    private async Task HandleEditAsync(MessageOperationEventArgs args)
    {
        if (_agent == null || _isStreaming || args.NewContent == null)
        {
            return;
        }

        // Validate index
        if (args.MessageIndex < 0 || args.MessageIndex >= _messages.Count)
        {
            return;
        }

        // Remove all messages from the edit point onwards
        while (_messages.Count > args.MessageIndex)
        {
            _messages.RemoveAt(_messages.Count - 1);
        }

        Logger.LogDebug("Editing message at index {MessageIndex}, new content: {ContentPreview}",
            args.MessageIndex, args.NewContent.Length > 50 ? args.NewContent[..50] + "..." : args.NewContent);

        StateHasChanged();

        // Resend with the edited message
        await ProcessAgentResponseAsync(args.NewContent);
    }

    #endregion

    #region Keyboard Shortcuts

    /// <summary>
    /// Handles keyboard events and executes matching shortcuts.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (_keyboardShortcutService == null)
        {
            return;
        }

        var shortcut = _keyboardShortcutService.FindMatch(
            args.Key,
            args.CtrlKey,
            args.AltKey,
            args.ShiftKey,
            args.MetaKey);

        if (shortcut == null)
        {
            return;
        }

        Logger.LogDebug("Keyboard shortcut matched: {Action}", shortcut.Action);

        switch (shortcut.Action)
        {
            case "cancel":
                if (_isStreaming)
                {
                    CancelStreaming();
                }
                break;

            case "new-chat":
                if (OnNewChat.HasDelegate)
                {
                    await OnNewChat.InvokeAsync();
                }
                else
                {
                    ClearConversation();
                }
                break;

            case "show-shortcuts":
                await ShowKeyboardShortcutsAsync();
                break;

            // Note: send-inline is handled by MudMessageInput
            // Note: previous-message and next-message can be handled by MudMessageList
        }
    }

    /// <summary>
    /// Shows the keyboard shortcuts help dialog.
    /// </summary>
    public async Task ShowKeyboardShortcutsAsync()
    {
        if (_keyboardShortcutService == null)
        {
            Logger.LogWarning("Cannot show keyboard shortcuts - IKeyboardShortcutService is not registered");
            return;
        }

        var parameters = new DialogParameters<MudKeyboardShortcutsHelp>
        {
            { x => x.ShortcutsByCategory, _keyboardShortcutService.ShortcutsByCategory },
            { x => x.IsMac, _keyboardShortcutService.IsMac }
        };

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        await DialogService.ShowAsync<MudKeyboardShortcutsHelp>("Keyboard Shortcuts", parameters, options);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the CSS class for the container element.
    /// </summary>
    /// <returns>The combined CSS class string.</returns>
    private string GetContainerClass()
    {
        var baseClass = "mud-agent-chat";
        return string.IsNullOrWhiteSpace(CssClass)
            ? baseClass
            : $"{baseClass} {CssClass}";
    }

    /// <summary>
    /// Gets the empty message text for the message list.
    /// </summary>
    /// <returns>The empty message text.</returns>
    private string GetEmptyMessage()
    {
        if (_connectionState == ConnectionState.Connecting)
        {
            return $"Connecting to {AgentName}...";
        }

        if (_connectionState == ConnectionState.Error || _agent == null)
        {
            return "Unable to connect to the agent.";
        }

        return $"Start a conversation with {AgentName}!";
    }

    /// <summary>
    /// Gets the placeholder text for the input field.
    /// </summary>
    /// <returns>The placeholder text.</returns>
    private string GetInputPlaceholder()
    {
        if (_connectionState == ConnectionState.Error || _connectionState == ConnectionState.Disconnected || _agent == null)
        {
            return "Agent unavailable — type /help for commands...";
        }

        if (_isStreaming)
        {
            return "Type to queue a message...";
        }

        return "Type a message...";
    }

    #endregion
}
