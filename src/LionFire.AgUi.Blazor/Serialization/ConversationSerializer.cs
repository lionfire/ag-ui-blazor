using System.Text.Json;
using System.Text.Json.Serialization;
using LionFire.AgUi.Blazor.Models;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization for <see cref="Conversation"/> objects.
/// </summary>
public static class ConversationSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    /// <summary>
    /// Creates the default JSON serialization options.
    /// </summary>
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new ChatMessageConverter(),
                new AIContentConverter()
            }
        };
        return options;
    }

    /// <summary>
    /// Serializes a conversation to JSON.
    /// </summary>
    /// <param name="conversation">The conversation to serialize.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>The JSON representation of the conversation.</returns>
    public static string Serialize(Conversation conversation, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(conversation, options ?? DefaultOptions);
    }

    /// <summary>
    /// Deserializes a conversation from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>The deserialized conversation, or null if deserialization fails.</returns>
    public static Conversation? Deserialize(string json, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<Conversation>(json, options ?? DefaultOptions);
    }

    /// <summary>
    /// Calculates the approximate size in bytes of a serialized conversation.
    /// </summary>
    /// <param name="conversation">The conversation to measure.</param>
    /// <returns>The approximate size in bytes.</returns>
    public static int GetApproximateSize(Conversation conversation)
    {
        return Serialize(conversation).Length * 2; // UTF-16 approximation
    }
}

/// <summary>
/// Custom JSON converter for <see cref="ChatMessage"/>.
/// </summary>
internal sealed class ChatMessageConverter : JsonConverter<ChatMessage>
{
    public override ChatMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var role = root.TryGetProperty("role", out var roleProp)
            ? ParseChatRole(roleProp.GetString() ?? "user")
            : ChatRole.User;

        var contents = new List<AIContent>();

        if (root.TryGetProperty("contents", out var contentsProp) && contentsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in contentsProp.EnumerateArray())
            {
                var content = DeserializeContent(item);
                if (content != null)
                    contents.Add(content);
            }
        }
        else if (root.TryGetProperty("text", out var textProp))
        {
            // Simplified format: just text
            contents.Add(new TextContent(textProp.GetString() ?? string.Empty));
        }

        var message = new ChatMessage(role, contents);

        if (root.TryGetProperty("authorName", out var authorProp))
            message.AuthorName = authorProp.GetString();

        return message;
    }

    private static ChatRole ParseChatRole(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "user" => ChatRole.User,
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => new ChatRole(value)
        };
    }

    private static AIContent? DeserializeContent(JsonElement element)
    {
        var type = element.TryGetProperty("type", out var typeProp)
            ? typeProp.GetString()
            : "text";

        return type switch
        {
            "text" => new TextContent(element.GetProperty("text").GetString() ?? string.Empty),
            "functionCall" => new FunctionCallContent(
                element.GetProperty("callId").GetString() ?? string.Empty,
                element.GetProperty("name").GetString() ?? string.Empty,
                element.TryGetProperty("arguments", out var args) ? args.Deserialize<IDictionary<string, object?>>() : null),
            "functionResult" => new FunctionResultContent(
                element.GetProperty("callId").GetString() ?? string.Empty,
                element.TryGetProperty("name", out var funcName) ? funcName.GetString() ?? string.Empty : string.Empty,
                element.TryGetProperty("result", out var result) ? result.Deserialize<object>() : null),
            _ => new TextContent(element.ToString()) // Fallback
        };
    }

    public override void Write(Utf8JsonWriter writer, ChatMessage value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("role", value.Role.Value);

        if (!string.IsNullOrEmpty(value.AuthorName))
            writer.WriteString("authorName", value.AuthorName);

        writer.WritePropertyName("contents");
        writer.WriteStartArray();

        foreach (var content in value.Contents)
        {
            SerializeContent(writer, content);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void SerializeContent(Utf8JsonWriter writer, AIContent content)
    {
        writer.WriteStartObject();

        switch (content)
        {
            case TextContent textContent:
                writer.WriteString("type", "text");
                writer.WriteString("text", textContent.Text);
                break;

            case FunctionCallContent functionCall:
                writer.WriteString("type", "functionCall");
                writer.WriteString("callId", functionCall.CallId);
                writer.WriteString("name", functionCall.Name);
                if (functionCall.Arguments != null)
                {
                    writer.WritePropertyName("arguments");
                    JsonSerializer.Serialize(writer, functionCall.Arguments);
                }
                break;

            case FunctionResultContent functionResult:
                writer.WriteString("type", "functionResult");
                writer.WriteString("callId", functionResult.CallId);
                writer.WriteString("name", functionResult.Name ?? string.Empty);
                if (functionResult.Result != null)
                {
                    writer.WritePropertyName("result");
                    JsonSerializer.Serialize(writer, functionResult.Result);
                }
                break;

            default:
                writer.WriteString("type", "unknown");
                writer.WriteString("data", content.ToString());
                break;
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Custom JSON converter for <see cref="AIContent"/>.
/// </summary>
internal sealed class AIContentConverter : JsonConverter<AIContent>
{
    public override AIContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var type = root.TryGetProperty("type", out var typeProp)
            ? typeProp.GetString()
            : "text";

        return type switch
        {
            "text" => new TextContent(root.GetProperty("text").GetString() ?? string.Empty),
            "functionCall" => new FunctionCallContent(
                root.GetProperty("callId").GetString() ?? string.Empty,
                root.GetProperty("name").GetString() ?? string.Empty,
                root.TryGetProperty("arguments", out var args) ? args.Deserialize<IDictionary<string, object?>>() : null),
            "functionResult" => new FunctionResultContent(
                root.GetProperty("callId").GetString() ?? string.Empty,
                root.TryGetProperty("name", out var funcName) ? funcName.GetString() ?? string.Empty : string.Empty,
                root.TryGetProperty("result", out var result) ? result.Deserialize<object>() : null),
            _ => new TextContent(root.ToString())
        };
    }

    public override void Write(Utf8JsonWriter writer, AIContent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case TextContent textContent:
                writer.WriteString("type", "text");
                writer.WriteString("text", textContent.Text);
                break;

            case FunctionCallContent functionCall:
                writer.WriteString("type", "functionCall");
                writer.WriteString("callId", functionCall.CallId);
                writer.WriteString("name", functionCall.Name);
                if (functionCall.Arguments != null)
                {
                    writer.WritePropertyName("arguments");
                    JsonSerializer.Serialize(writer, functionCall.Arguments, options);
                }
                break;

            case FunctionResultContent functionResult:
                writer.WriteString("type", "functionResult");
                writer.WriteString("callId", functionResult.CallId);
                writer.WriteString("name", functionResult.Name ?? string.Empty);
                if (functionResult.Result != null)
                {
                    writer.WritePropertyName("result");
                    JsonSerializer.Serialize(writer, functionResult.Result, options);
                }
                break;

            default:
                writer.WriteString("type", "unknown");
                writer.WriteString("data", value.ToString());
                break;
        }

        writer.WriteEndObject();
    }
}
