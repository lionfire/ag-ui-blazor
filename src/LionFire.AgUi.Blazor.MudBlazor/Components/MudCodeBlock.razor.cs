using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A code block component with syntax highlighting and copy-to-clipboard functionality.
/// </summary>
/// <remarks>
/// <para>
/// This component displays code with optional syntax highlighting using highlight.js
/// and provides a copy-to-clipboard button. It integrates with MudBlazor theming
/// for consistent styling across light and dark modes.
/// </para>
/// <para>
/// <strong>Usage Example:</strong>
/// </para>
/// <code>
/// &lt;MudCodeBlock Code="@csharpCode" Language="csharp" ShowCopyButton="true" /&gt;
/// </code>
/// </remarks>
public partial class MudCodeBlock : ComponentBase, IAsyncDisposable
{
    private ElementReference _codeElement;
    private IJSObjectReference? _highlightModule;
    private bool _hasHighlighted;
    private string? _previousCode;
    private string? _previousLanguage;

    // MudBlazor constants for use in razor template
    protected static string CopyIcon => Icons.Material.Filled.ContentCopy;
    protected static Size SmallSize => Size.Small;
    protected static Color DefaultColor => Color.Default;

    /// <summary>
    /// Gets or sets the code content to display.
    /// </summary>
    [Parameter]
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets the programming language for syntax highlighting.
    /// Common values: csharp, javascript, python, sql, json, xml, bash.
    /// </summary>
    [Parameter]
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets whether to show a copy-to-clipboard button.
    /// Default is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ShowCopyButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show line numbers.
    /// Default is <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ShowLineNumbers { get; set; }

    /// <summary>
    /// Gets or sets whether to enable syntax highlighting.
    /// Default is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool EnableHighlighting { get; set; } = true;

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
    /// Event raised when the code is copied to clipboard.
    /// The event argument contains the copied code.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnCopied { get; set; }

    /// <summary>
    /// Event raised when copying fails.
    /// The event argument contains the error message.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnCopyFailed { get; set; }

    /// <summary>
    /// Gets the CSS class for the pre element.
    /// </summary>
    protected string PreClass => ShowLineNumbers ? "line-numbers" : string.Empty;

    /// <summary>
    /// Gets the CSS class for the code element.
    /// </summary>
    protected string CodeClass => !string.IsNullOrEmpty(Language) ? $"language-{Language}" : string.Empty;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var needsHighlight = EnableHighlighting &&
                             !string.IsNullOrEmpty(Code) &&
                             (!_hasHighlighted || Code != _previousCode || Language != _previousLanguage);

        if (needsHighlight)
        {
            _previousCode = Code;
            _previousLanguage = Language;
            _hasHighlighted = true;

            try
            {
                _highlightModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./_content/LionFire.AgUi.Blazor.MudBlazor/js/highlight-interop.js"
                );

                await _highlightModule.InvokeVoidAsync("highlightElement", _codeElement);
            }
            catch (JSException)
            {
                // Silently fail if highlight.js is not available
            }
        }
    }

    /// <summary>
    /// Handles the copy button click event.
    /// </summary>
    protected async Task HandleCopyClick()
    {
        if (string.IsNullOrEmpty(Code))
        {
            return;
        }

        try
        {
            _highlightModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/LionFire.AgUi.Blazor.MudBlazor/js/highlight-interop.js"
            );

            var success = await _highlightModule.InvokeAsync<bool>("copyToClipboard", Code);

            if (success)
            {
                await OnCopied.InvokeAsync(Code);
            }
            else
            {
                await OnCopyFailed.InvokeAsync("Failed to copy to clipboard");
            }
        }
        catch (JSException ex)
        {
            await OnCopyFailed.InvokeAsync(ex.Message);
        }
    }

    /// <summary>
    /// Gets a display-friendly name for the programming language.
    /// </summary>
    protected static string GetDisplayLanguage(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "csharp" or "cs" => "C#",
            "javascript" or "js" => "JavaScript",
            "typescript" or "ts" => "TypeScript",
            "python" or "py" => "Python",
            "ruby" or "rb" => "Ruby",
            "bash" or "sh" or "shell" => "Bash",
            "powershell" or "ps1" => "PowerShell",
            "sql" => "SQL",
            "json" => "JSON",
            "xml" => "XML",
            "yaml" or "yml" => "YAML",
            "html" or "htm" => "HTML",
            "css" => "CSS",
            "scss" => "SCSS",
            "less" => "LESS",
            "java" => "Java",
            "kotlin" or "kt" => "Kotlin",
            "swift" => "Swift",
            "go" => "Go",
            "rust" or "rs" => "Rust",
            "cpp" or "c++" => "C++",
            "c" => "C",
            "php" => "PHP",
            "markdown" or "md" => "Markdown",
            "dockerfile" or "docker" => "Dockerfile",
            "plaintext" or "text" => "Plain Text",
            _ => language.ToUpperInvariant()
        };
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
