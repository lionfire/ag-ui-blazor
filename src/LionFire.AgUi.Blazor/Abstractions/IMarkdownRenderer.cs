using Microsoft.AspNetCore.Components;

namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Interface for rendering Markdown content to HTML.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts Markdown rendering, allowing different implementations
/// to be used (e.g., Markdig, Markdownsharp, or custom renderers).
/// </para>
/// <para>
/// Implementations should handle common Markdown features including:
/// headings, bold, italic, lists, links, tables, code blocks, and blockquotes.
/// </para>
/// <para>
/// <strong>Security Note:</strong>
/// Implementations should sanitize output to prevent XSS attacks when rendering
/// user-provided Markdown content.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example usage in a Blazor component
/// @inject IMarkdownRenderer MarkdownRenderer
///
/// var html = MarkdownRenderer.RenderHtml("# Hello World");
/// // or
/// MarkupString markup = MarkdownRenderer.RenderMarkup("**Bold text**");
/// </code>
/// </example>
public interface IMarkdownRenderer
{
    /// <summary>
    /// Renders Markdown content to an HTML string.
    /// </summary>
    /// <param name="markdown">The Markdown content to render.</param>
    /// <returns>The rendered HTML string, or an empty string if input is null or empty.</returns>
    /// <remarks>
    /// The returned HTML should be sanitized to prevent XSS attacks.
    /// </remarks>
    string RenderHtml(string? markdown);

    /// <summary>
    /// Renders Markdown content to a <see cref="MarkupString"/> for direct use in Blazor components.
    /// </summary>
    /// <param name="markdown">The Markdown content to render.</param>
    /// <returns>A <see cref="MarkupString"/> containing the rendered HTML.</returns>
    /// <remarks>
    /// This method is a convenience wrapper around <see cref="RenderHtml"/> that
    /// returns a <see cref="MarkupString"/> which can be directly rendered in Razor templates.
    /// </remarks>
    MarkupString RenderMarkup(string? markdown);

    /// <summary>
    /// Extracts code blocks from Markdown content.
    /// </summary>
    /// <param name="markdown">The Markdown content to parse.</param>
    /// <returns>A collection of code blocks found in the Markdown content.</returns>
    IReadOnlyList<CodeBlock> ExtractCodeBlocks(string? markdown);
}

/// <summary>
/// Represents a code block extracted from Markdown content.
/// </summary>
/// <param name="Code">The code content.</param>
/// <param name="Language">The programming language specified in the code fence (e.g., "csharp", "javascript"), or null if not specified.</param>
/// <param name="StartIndex">The starting character index of the code block in the original Markdown.</param>
/// <param name="EndIndex">The ending character index of the code block in the original Markdown.</param>
public record CodeBlock(string Code, string? Language, int StartIndex, int EndIndex);
