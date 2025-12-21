using System.Text;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Serialization;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// Default implementation of <see cref="IConversationExporter"/>.
/// </summary>
public sealed class ConversationExporter : IConversationExporter
{

    /// <inheritdoc />
    public string Export(Conversation conversation, ExportFormat format)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        return format switch
        {
            ExportFormat.Json => ExportToJson(conversation),
            ExportFormat.Markdown => ExportToMarkdown(conversation),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format")
        };
    }

    /// <inheritdoc />
    public Conversation ImportFromJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content cannot be empty", nameof(content));
        }

        return ConversationSerializer.Deserialize(content)
            ?? throw new InvalidOperationException("Failed to deserialize conversation");
    }

    /// <inheritdoc />
    public string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Json => ".json",
            ExportFormat.Markdown => ".md",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format")
        };
    }

    /// <inheritdoc />
    public string GetMimeType(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Json => "application/json",
            ExportFormat.Markdown => "text/markdown",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format")
        };
    }

    private static string ExportToJson(Conversation conversation)
    {
        return ConversationSerializer.Serialize(conversation);
    }

    private static string ExportToMarkdown(Conversation conversation)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"# Conversation with {conversation.AgentName}");
        sb.AppendLine();
        sb.AppendLine($"**Created**: {conversation.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Last Modified**: {conversation.LastModifiedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Messages**: {conversation.Messages.Count}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Messages
        foreach (var message in conversation.Messages)
        {
            var roleName = GetRoleDisplayName(message.Role);
            sb.AppendLine($"## {roleName}");
            sb.AppendLine();
            sb.AppendLine(message.Text ?? "(empty message)");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetRoleDisplayName(ChatRole role)
    {
        return role.Value switch
        {
            "user" => "You",
            "assistant" => "Assistant",
            "system" => "System",
            "tool" => "Tool",
            _ => role.Value
        };
    }
}
