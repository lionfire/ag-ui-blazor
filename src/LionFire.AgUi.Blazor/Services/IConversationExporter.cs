using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// Export format for conversations.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// JSON format with full message details.
    /// </summary>
    Json,

    /// <summary>
    /// Markdown format for human-readable export.
    /// </summary>
    Markdown
}

/// <summary>
/// Service for exporting and importing conversations.
/// </summary>
public interface IConversationExporter
{
    /// <summary>
    /// Exports a conversation to the specified format.
    /// </summary>
    /// <param name="conversation">The conversation to export.</param>
    /// <param name="format">The export format.</param>
    /// <returns>The exported content as a string.</returns>
    string Export(Conversation conversation, ExportFormat format);

    /// <summary>
    /// Imports a conversation from JSON content.
    /// </summary>
    /// <param name="content">The JSON content to import.</param>
    /// <returns>The imported conversation.</returns>
    Conversation ImportFromJson(string content);

    /// <summary>
    /// Gets the file extension for the specified format.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <returns>The file extension including the dot.</returns>
    string GetFileExtension(ExportFormat format);

    /// <summary>
    /// Gets the MIME type for the specified format.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <returns>The MIME type.</returns>
    string GetMimeType(ExportFormat format);
}
