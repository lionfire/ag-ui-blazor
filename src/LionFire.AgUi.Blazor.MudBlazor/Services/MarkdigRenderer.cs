using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.AspNetCore.Components;
using IMarkdownRenderer = LionFire.AgUi.Blazor.Abstractions.IMarkdownRenderer;
using CodeBlockModel = LionFire.AgUi.Blazor.Abstractions.CodeBlock;
using MarkdigCodeBlock = Markdig.Syntax.CodeBlock;

namespace LionFire.AgUi.Blazor.MudBlazor.Services;

/// <summary>
/// Markdown renderer implementation using Markdig with common extensions.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses Markdig with the following extensions enabled:
/// <list type="bullet">
///   <item><description>Tables - Standard and pipe tables</description></item>
///   <item><description>Task lists - GitHub-style task lists</description></item>
///   <item><description>Auto-links - Automatic URL and email linking</description></item>
///   <item><description>Extra emphasis - Strikethrough, subscript, superscript</description></item>
///   <item><description>Footnotes - Reference-style footnotes</description></item>
/// </list>
/// </para>
/// <para>
/// Code blocks are rendered with special CSS classes for integration with
/// highlight.js or Prism.js syntax highlighting.
/// </para>
/// </remarks>
public sealed partial class MarkdigRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdigRenderer"/> class.
    /// </summary>
    public MarkdigRenderer()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoIdentifiers()
            .UseTaskLists()
            .UseAutoLinks()
            .UsePipeTables()
            .UseFootnotes()
            .UseGenericAttributes()
            .Build();
    }

    /// <inheritdoc />
    public string RenderHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var document = Markdown.Parse(markdown, _pipeline);

        // Add language classes to code blocks for syntax highlighting
        foreach (var codeBlock in document.Descendants<FencedCodeBlock>())
        {
            var language = codeBlock.Info;
            if (!string.IsNullOrEmpty(language))
            {
                // Normalize common language aliases
                language = NormalizeLanguage(language);

                var attributes = codeBlock.GetAttributes();
                attributes.AddClass($"language-{language}");
                attributes.AddProperty("data-language", language);
            }
        }

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer)
        {
            EnableHtmlForBlock = true,
            EnableHtmlForInline = true
        };
        _pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();

        var html = writer.ToString();

        // Basic XSS sanitization - remove script tags and javascript: URLs
        html = SanitizeHtml(html);

        return html;
    }

    /// <inheritdoc />
    public MarkupString RenderMarkup(string? markdown)
    {
        return new MarkupString(RenderHtml(markdown));
    }

    /// <inheritdoc />
    public IReadOnlyList<CodeBlockModel> ExtractCodeBlocks(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return Array.Empty<CodeBlockModel>();
        }

        var document = Markdown.Parse(markdown, _pipeline);
        var codeBlocks = new List<CodeBlockModel>();

        foreach (var block in document.Descendants<FencedCodeBlock>())
        {
            var code = GetCodeBlockContent(block);
            var language = string.IsNullOrEmpty(block.Info) ? null : NormalizeLanguage(block.Info);

            codeBlocks.Add(new CodeBlockModel(
                code,
                language,
                block.Span.Start,
                block.Span.End
            ));
        }

        // Also extract indented code blocks
        foreach (var block in document.Descendants<MarkdigCodeBlock>())
        {
            if (block is not FencedCodeBlock) // Already handled fenced blocks
            {
                var code = GetCodeBlockContent(block);
                codeBlocks.Add(new CodeBlockModel(
                    code,
                    null, // Indented blocks don't have language info
                    block.Span.Start,
                    block.Span.End
                ));
            }
        }

        return codeBlocks.OrderBy(b => b.StartIndex).ToList();
    }

    private static string GetCodeBlockContent(MarkdigCodeBlock block)
    {
        var lines = block.Lines;
        var code = new StringWriter();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines.Lines[i];
            code.Write(line.Slice);
            if (i < lines.Count - 1)
            {
                code.WriteLine();
            }
        }

        return code.ToString();
    }

    /// <summary>
    /// Normalizes language identifiers to their canonical forms.
    /// </summary>
    private static string NormalizeLanguage(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "cs" or "c#" => "csharp",
            "js" => "javascript",
            "ts" => "typescript",
            "py" => "python",
            "rb" => "ruby",
            "sh" or "bash" or "shell" => "bash",
            "yml" => "yaml",
            "md" => "markdown",
            "html" or "htm" => "html",
            "xml" or "xsl" or "xslt" => "xml",
            "ps1" or "powershell" => "powershell",
            "dockerfile" => "docker",
            _ => language.ToLowerInvariant()
        };
    }

    /// <summary>
    /// Performs basic HTML sanitization to prevent XSS attacks.
    /// </summary>
    private static string SanitizeHtml(string html)
    {
        // Remove script tags
        html = ScriptTagRegex().Replace(html, string.Empty);

        // Remove javascript: URLs
        html = JavascriptUrlRegex().Replace(html, "href=\"\"");

        // Remove on* event handlers
        html = OnEventRegex().Replace(html, string.Empty);

        return html;
    }

    [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex(@"href\s*=\s*[""']javascript:[^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex JavascriptUrlRegex();

    [GeneratedRegex(@"\s+on\w+\s*=\s*[""'][^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex OnEventRegex();
}
