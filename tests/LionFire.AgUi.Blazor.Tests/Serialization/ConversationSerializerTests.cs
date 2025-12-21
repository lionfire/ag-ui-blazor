using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Serialization;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Tests.Serialization;

public class ConversationSerializerTests
{
    [Fact]
    public void Serialize_EmptyConversation_ProducesValidJson()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent");

        // Act
        var json = ConversationSerializer.Serialize(conversation);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"agentName\":\"test-agent\"", json);
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsConversation()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent");
        var json = ConversationSerializer.Serialize(conversation);

        // Act
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conversation.Id, result.Id);
        Assert.Equal(conversation.AgentName, result.AgentName);
    }

    [Fact]
    public void RoundTrip_WithMessages_PreservesData()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent")
            .WithMessage(new ChatMessage(ChatRole.User, "Hello"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "Hi there!"));

        // Act
        var json = ConversationSerializer.Serialize(conversation);
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);
        Assert.Equal(ChatRole.User, result.Messages[0].Role);
        Assert.Equal(ChatRole.Assistant, result.Messages[1].Role);
    }

    [Fact]
    public void RoundTrip_WithTextContent_PreservesText()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.User, "Hello, world!");
        var conversation = Conversation.Create("test-agent").WithMessage(message);

        // Act
        var json = ConversationSerializer.Serialize(conversation);
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Messages);
        var content = result.Messages[0].Contents.FirstOrDefault();
        Assert.IsType<TextContent>(content);
        Assert.Equal("Hello, world!", ((TextContent)content).Text);
    }

    [Fact]
    public void RoundTrip_PreservesTimestamps()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent");

        // Act
        var json = ConversationSerializer.Serialize(conversation);
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conversation.CreatedAt.ToString("O"), result.CreatedAt.ToString("O"));
        Assert.Equal(conversation.LastModifiedAt.ToString("O"), result.LastModifiedAt.ToString("O"));
    }

    [Fact]
    public void Deserialize_NullOrEmpty_ReturnsNull()
    {
        // Act & Assert
        Assert.Null(ConversationSerializer.Deserialize(null!));
        Assert.Null(ConversationSerializer.Deserialize(""));
        Assert.Null(ConversationSerializer.Deserialize("   "));
    }

    [Fact]
    public void GetApproximateSize_ReturnsPositiveValue()
    {
        // Arrange
        var conversation = Conversation.Create("test-agent")
            .WithMessage(new ChatMessage(ChatRole.User, "Hello, world!"));

        // Act
        var size = ConversationSerializer.GetApproximateSize(conversation);

        // Assert
        Assert.True(size > 0);
    }

    [Fact]
    public void GetApproximateSize_IncreasesWithMessages()
    {
        // Arrange
        var conv1 = Conversation.Create("test-agent");
        var conv2 = conv1.WithMessage(new ChatMessage(ChatRole.User, "Hello"));
        var conv3 = conv2.WithMessage(new ChatMessage(ChatRole.Assistant, "Hi there!"));

        // Act
        var size1 = ConversationSerializer.GetApproximateSize(conv1);
        var size2 = ConversationSerializer.GetApproximateSize(conv2);
        var size3 = ConversationSerializer.GetApproximateSize(conv3);

        // Assert
        Assert.True(size2 > size1);
        Assert.True(size3 > size2);
    }

    [Fact]
    public void RoundTrip_WithAuthorName_PreservesAuthorName()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.Assistant, "Hello!")
        {
            AuthorName = "Claude"
        };
        var conversation = Conversation.Create("test-agent").WithMessage(message);

        // Act
        var json = ConversationSerializer.Serialize(conversation);
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Claude", result.Messages[0].AuthorName);
    }

    [Fact]
    public void RoundTrip_WithFunctionCallContent_PreservesData()
    {
        // Arrange
        var functionCall = new FunctionCallContent("call-123", "get_weather",
            new Dictionary<string, object?> { { "city", "Seattle" } });
        var message = new ChatMessage(ChatRole.Assistant, [functionCall]);
        var conversation = Conversation.Create("test-agent").WithMessage(message);

        // Act
        var json = ConversationSerializer.Serialize(conversation);
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        var content = result.Messages[0].Contents.FirstOrDefault();
        Assert.IsType<FunctionCallContent>(content);
        var resultCall = (FunctionCallContent)content;
        Assert.Equal("call-123", resultCall.CallId);
        Assert.Equal("get_weather", resultCall.Name);
    }

    [Fact]
    public void RoundTrip_WithFunctionResultContent_PreservesData()
    {
        // Arrange
        var functionResult = new FunctionResultContent("call-123", "get_weather", "Sunny, 72°F");
        var message = new ChatMessage(ChatRole.Tool, [functionResult]);
        var conversation = Conversation.Create("test-agent").WithMessage(message);

        // Act
        var json = ConversationSerializer.Serialize(conversation);
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        var content = result.Messages[0].Contents.FirstOrDefault();
        Assert.IsType<FunctionResultContent>(content);
        var resultContent = (FunctionResultContent)content;
        Assert.Equal("call-123", resultContent.CallId);
    }

    [Fact]
    public void CreateDefaultOptions_ReturnsConfiguredOptions()
    {
        // Act
        var options = ConversationSerializer.CreateDefaultOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(System.Text.Json.JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
        Assert.False(options.WriteIndented);
    }

    [Fact]
    public void RoundTrip_MultipleMessageTypes_PreservesAll()
    {
        // Arrange
        var conversation = Conversation.Create("assistant")
            .WithMessage(new ChatMessage(ChatRole.System, "You are a helpful assistant."))
            .WithMessage(new ChatMessage(ChatRole.User, "What's the weather?"))
            .WithMessage(new ChatMessage(ChatRole.Assistant, [
                new FunctionCallContent("call-1", "get_weather",
                    new Dictionary<string, object?> { { "city", "Seattle" } })
            ]))
            .WithMessage(new ChatMessage(ChatRole.Tool, [
                new FunctionResultContent("call-1", "get_weather", "Sunny, 72°F")
            ]))
            .WithMessage(new ChatMessage(ChatRole.Assistant, "The weather in Seattle is sunny and 72°F."));

        // Act
        var json = ConversationSerializer.Serialize(conversation);
        var result = ConversationSerializer.Deserialize(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Messages.Count);
        Assert.Equal(ChatRole.System, result.Messages[0].Role);
        Assert.Equal(ChatRole.User, result.Messages[1].Role);
        Assert.Equal(ChatRole.Assistant, result.Messages[2].Role);
        Assert.Equal(ChatRole.Tool, result.Messages[3].Role);
        Assert.Equal(ChatRole.Assistant, result.Messages[4].Role);
    }
}
