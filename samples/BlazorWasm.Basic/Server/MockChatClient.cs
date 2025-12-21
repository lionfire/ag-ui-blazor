using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace BlazorWasm.Basic.Server;

/// <summary>
/// A mock implementation of <see cref="IChatClient"/> for demonstration purposes.
/// Returns predefined streaming responses without requiring any external API.
/// </summary>
public class MockChatClient : IChatClient
{
    private readonly string[] _defaultResponses =
    [
        "Hello! I'm a demo AI assistant running in Blazor WebAssembly. ",
        "I'm communicating with the server over HTTP/SSE for streaming responses. ",
        "This demonstrates the AG-UI protocol in action. ",
        "Try asking me about Blazor WASM or offline support!"
    ];

    private readonly Dictionary<string, string[]> _contextualResponses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hello"] = ["Hi there! ", "How can I help you today?"],
        ["hi"] = ["Hello! ", "Nice to meet you. ", "What would you like to know?"],
        ["how are you"] = ["I'm doing great, thanks for asking! ", "Ready to help you explore Blazor WASM."],
        ["wasm"] = [
            "Blazor WebAssembly runs entirely in the browser using .NET! ",
            "This means your C# code compiles to WebAssembly and runs client-side. ",
            "The chat UI you're using right now is running in WASM, ",
            "while the AI responses come from the server via HTTP streaming."
        ],
        ["offline"] = [
            "The AG-UI WASM package includes offline support! ",
            "When you lose connectivity, messages are queued locally. ",
            "Once you're back online, they're automatically sent to the server. ",
            "Try disconnecting your network to see it in action!"
        ],
        ["blazor"] = [
            "Blazor is amazing for building interactive web UIs with C#! ",
            "This demo shows Blazor WebAssembly architecture: ",
            "- Client (WASM): Runs the UI entirely in the browser ",
            "- Server: Provides the AI chat API endpoint ",
            "- Shared: Common models used by both projects"
        ],
        ["bye"] = ["Goodbye! ", "Have a great day!"],
        ["thanks"] = ["You're welcome! ", "Let me know if you need anything else."],
        ["help"] = [
            "I can answer questions about: ",
            "- Blazor WebAssembly architecture ",
            "- Offline support and message queuing ",
            "- The AG-UI protocol and components ",
            "Just ask away!"
        ]
    };

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Simulate initial processing delay
        await Task.Delay(200, cancellationToken);

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
                await Task.Delay(12, cancellationToken);
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
        await Task.Delay(200, cancellationToken);

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
        modelId: "demo-wasm-model-v1");

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
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
