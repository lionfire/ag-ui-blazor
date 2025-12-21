using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace BlazorServer.Basic;

/// <summary>
/// A mock implementation of <see cref="IChatClient"/> for demonstration purposes.
/// Returns predefined streaming responses without requiring any external API.
/// </summary>
public class MockChatClient : IChatClient
{
    private readonly string[] _defaultResponses =
    [
        "Hello! I'm a demo AI assistant. ",
        "I'm running directly in your Blazor Server app ",
        "with sub-millisecond streaming latency. ",
        "Try asking me anything!"
    ];

    private readonly Dictionary<string, string[]> _contextualResponses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hello"] = ["Hi there! ", "How can I help you today?"],
        ["hi"] = ["Hello! ", "Nice to meet you. ", "What would you like to know?"],
        ["how are you"] = ["I'm doing great, thanks for asking! ", "I'm here and ready to help you."],
        ["what can you do"] = [
            "I'm a demo AI assistant that demonstrates the AG-UI Blazor components. ",
            "In a real application, you could connect me to OpenAI, Anthropic, ",
            "or any other AI service that implements IChatClient."
        ],
        ["mudblazor"] = [
            "MudBlazor is a Material Design component library for Blazor. ",
            "This chat interface is built using MudBlazor components, ",
            "providing a clean, modern UI with built-in theming support."
        ],
        ["blazor"] = [
            "Blazor is a fantastic framework for building interactive web UIs using C#! ",
            "This demo uses Blazor Server with SignalR for real-time communication, ",
            "which means your messages stream directly from the server with minimal latency."
        ],
        ["bye"] = ["Goodbye! ", "Have a great day!"],
        ["thanks"] = ["You're welcome! ", "Let me know if you need anything else."],
        ["help"] = [
            "I can answer questions about Blazor, MudBlazor, or this demo application. ",
            "Try asking me about the technology stack or how this sample works!"
        ]
    };

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Simulate initial processing delay
        await Task.Delay(300, cancellationToken);

        // Get the last user message
        var lastUserMessage = chatMessages
            .LastOrDefault(m => m.Role == ChatRole.User)
            ?.Text ?? string.Empty;

        // Choose appropriate response based on message content
        var responses = GetResponses(lastUserMessage);

        foreach (var response in responses)
        {
            foreach (char c in response)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return new StreamingChatCompletionUpdate
                {
                    Text = c.ToString()
                };

                // Simulate realistic streaming delay
                await Task.Delay(15, cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Simulate processing delay
        await Task.Delay(300, cancellationToken);

        var lastUserMessage = chatMessages
            .LastOrDefault(m => m.Role == ChatRole.User)
            ?.Text ?? string.Empty;

        var responses = GetResponses(lastUserMessage);
        var fullResponse = string.Join("", responses);

        return new ChatCompletion(new ChatMessage(ChatRole.Assistant, fullResponse));
    }

    /// <inheritdoc />
    public ChatClientMetadata Metadata => new(
        providerName: "MockChatClient",
        providerUri: new Uri("https://github.com/lionfire/ag-ui-blazor"),
        modelId: "demo-model-v1");

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // No resources to dispose
        GC.SuppressFinalize(this);
    }

    private string[] GetResponses(string userMessage)
    {
        // Check for keyword matches
        foreach (var (keyword, responses) in _contextualResponses)
        {
            if (userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return responses;
            }
        }

        // Return default response if no match
        return _defaultResponses;
    }
}
