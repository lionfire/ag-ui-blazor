using LionFire.AgUi.Blazor.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A Blazor component for rendering Markdown content with syntax highlighting support.
/// </summary>
/// <remarks>
/// <para>
/// This component uses <see cref="IMarkdownRenderer"/> to convert Markdown to HTML
/// and optionally integrates with highlight.js for syntax highlighting of code blocks.
/// </para>
/// <para>
/// <strong>Usage Example:</strong>
/// </para>
/// <code>
/// &lt;MudMarkdown Content="@markdownContent" ShowCopyButton="true" /&gt;
/// </code>
/// </remarks>
public partial class MudMarkdown : ComponentBase, IAsyncDisposable
{
    private MarkupString _renderedContent;
    private IJSObjectReference? _highlightModule;
    private string? _previousContent;

    /// <summary>
    /// Gets or sets the Markdown content to render.
    /// </summary>
    [Parameter]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets whether to show copy-to-clipboard buttons on code blocks.
    /// Default is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ShowCopyButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable syntax highlighting for code blocks.
    /// Default is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool EnableSyntaxHighlighting { get; set; } = true;

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
    /// Event raised when a code block's copy button is clicked.
    /// The event argument contains the code that was copied.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnCodeCopied { get; set; }

    /// <summary>
    /// The injected Markdown renderer service.
    /// </summary>
    [Inject]
    private IMarkdownRenderer MarkdownRenderer { get; set; } = null!;

    /// <summary>
    /// The injected JavaScript runtime for interop.
    /// </summary>
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Content != _previousContent)
        {
            _previousContent = Content;
            _renderedContent = MarkdownRenderer.RenderMarkup(Content);
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (EnableSyntaxHighlighting && !string.IsNullOrEmpty(Content))
        {
            try
            {
                _highlightModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./_content/LionFire.AgUi.Blazor.MudBlazor/js/highlight-interop.js"
                );

                await _highlightModule.InvokeVoidAsync("highlightAll");
            }
            catch (JSException)
            {
                // Silently fail if highlight.js is not available
                // This allows the component to work without syntax highlighting
            }
        }
    }

    /// <summary>
    /// Copies the specified code to the clipboard.
    /// </summary>
    /// <param name="code">The code to copy.</param>
    public async Task CopyToClipboardAsync(string code)
    {
        try
        {
            _highlightModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/LionFire.AgUi.Blazor.MudBlazor/js/highlight-interop.js"
            );

            await _highlightModule.InvokeVoidAsync("copyToClipboard", code);
            await OnCodeCopied.InvokeAsync(code);
        }
        catch (JSException)
        {
            // Handle clipboard errors gracefully
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_highlightModule is not null)
        {
            try
            {
                await _highlightModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit is disconnected, ignore
            }
        }
    }
}
