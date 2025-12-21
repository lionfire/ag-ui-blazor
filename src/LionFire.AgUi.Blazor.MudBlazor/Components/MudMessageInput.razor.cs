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
    /// Gets or sets whether dark mode is active. When set, adjusts input background color.
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
    /// Gets the inline style for the input container based on the theme.
    /// </summary>
    protected string GetContainerStyle()
    {
        var bgColor = EffectiveIsDarkMode
            ? "var(--mud-palette-gray-darker)"
            : "var(--mud-palette-gray-lighter)";
        return $"background-color: {bgColor};";
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
