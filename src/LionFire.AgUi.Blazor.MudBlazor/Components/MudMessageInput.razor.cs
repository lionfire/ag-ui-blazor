using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A message input component for chat interfaces with MudBlazor styling.
/// Supports Enter to send, Shift+Enter for new line, and auto-focus after send.
/// </summary>
public partial class MudMessageInput : ComponentBase, IAsyncDisposable
{
    private MudTextField<string>? _textField;
    private ElementReference _containerRef;
    private string _message = string.Empty;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MudMessageInput>? _dotNetRef;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets the inline style for the input container.
    /// Uses theme-aware CSS variable that adapts to dark/light mode automatically.
    /// </summary>
    protected string GetContainerStyle()
    {
        return "background-color: var(--mud-palette-surface);";
    }

    /// <summary>
    /// Whether a footer row is rendered beneath the text field.
    /// When false, the classic single-row layout (field + send button) is preserved.
    /// </summary>
    protected bool HasFooter => FooterStart is not null || FooterEnd is not null;

    /// <summary>
    /// Gets the CSS class for the input container, adding the footer variant when
    /// footer content is present.
    /// </summary>
    protected string GetContainerClass()
    {
        return HasFooter ? "mud-message-input has-footer" : "mud-message-input";
    }

    // MudBlazor constants for use in razor template
    protected static string SendIcon => Icons.Material.Filled.ArrowUpward;
    protected static Variant OutlinedVariant => Variant.Outlined;
    protected static Color PrimaryColor => Color.Primary;

    /// <summary>
    /// Called when the user sends a message (via Enter key or send button).
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSend { get; set; }

    /// <summary>
    /// Disables the input field and send button. Use during streaming responses.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Placeholder text displayed when the input is empty.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Type a message...";

    /// <summary>
    /// Optional content rendered at the left of a footer row beneath the text field
    /// (e.g., model or mode selectors supplied by the consuming application).
    /// When both <see cref="FooterStart"/> and <see cref="FooterEnd"/> are null,
    /// no footer row is rendered and the classic single-row layout is preserved.
    /// When either is set, the send button moves to the right end of the footer row.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterStart { get; set; }

    /// <summary>
    /// Optional content rendered right-aligned in the footer row, immediately before
    /// the send button. See <see cref="FooterStart"/> for layout semantics.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterEnd { get; set; }

    /// <summary>
    /// Gets the current message text. Useful for testing.
    /// </summary>
    public string Message => _message;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/LionFire.AgUi.Blazor.MudBlazor/js/message-input.js");
                _dotNetRef = DotNetObjectReference.Create(this);
                await _jsModule.InvokeVoidAsync("initializeMessageInput", _containerRef, _dotNetRef);
            }
            catch (JSException ex)
            {
                // Log but don't crash - fallback to button-only send
                Console.WriteLine($"MudMessageInput: Failed to initialize JS module: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Called from JavaScript when Enter is pressed (without Shift).
    /// </summary>
    [JSInvokable]
    public async Task HandleEnterPressed()
    {
        await SendMessage();
    }

    /// <summary>
    /// Sends the current message if it's not empty and the component is not disabled.
    /// Clears the input and refocuses after sending.
    /// </summary>
    public async Task SendMessage()
    {
        if (Disabled || string.IsNullOrWhiteSpace(_message))
        {
            return;
        }

        var messageToSend = _message.Trim();

        // Clear the input immediately via JavaScript
        _message = string.Empty;
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("clearInput", _containerRef);
        }

        await OnSend.InvokeAsync(messageToSend);
        StateHasChanged();
    }

    /// <summary>
    /// Sets focus to the input field.
    /// </summary>
    public async Task FocusAsync()
    {
        if (_textField is not null)
        {
            await _textField.FocusAsync();
        }
    }

    /// <summary>
    /// Clears the current message text.
    /// </summary>
    public void Clear()
    {
        _message = string.Empty;
        StateHasChanged();
    }

    /// <summary>
    /// Sets the message text programmatically.
    /// </summary>
    /// <param name="message">The message to set.</param>
    public void SetMessage(string message)
    {
        _message = message;
        StateHasChanged();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose", _containerRef);
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignore - browser has disconnected
            }
        }

        _dotNetRef?.Dispose();
    }
}
