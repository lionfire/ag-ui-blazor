using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace BlazorServer.Full;

/// <summary>
/// A mock implementation of <see cref="IChatClient"/> for demonstration purposes.
/// Returns predefined streaming responses without requiring any external API.
/// Supports multiple agent personalities for demonstrating multi-agent scenarios.
/// </summary>
public class MockChatClient : IChatClient
{
    private readonly string _agentType;

    private readonly Dictionary<string, string[]> _agentIntros = new()
    {
        ["assistant"] = [
            "Hello! I'm a demo AI assistant. ",
            "I'm running directly in your Blazor Server app ",
            "with sub-millisecond streaming latency. ",
            "Try asking me anything!"
        ],
        ["coder"] = [
            "Hi! I'm your coding assistant. ",
            "I can help you with programming questions, ",
            "code reviews, and debugging. ",
            "What would you like to work on?"
        ],
        ["researcher"] = [
            "Greetings! I'm a research assistant. ",
            "I specialize in finding information, ",
            "analyzing data, and summarizing findings. ",
            "What topic would you like to explore?"
        ]
    };

    private readonly Dictionary<string, Dictionary<string, string[]>> _contextualResponses = new()
    {
        ["assistant"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["hello"] = ["Hi there! ", "How can I help you today?"],
            ["hi"] = ["Hello! ", "Nice to meet you. ", "What would you like to know?"],
            ["keyboard"] = [
                "You can use keyboard shortcuts! ",
                "Try Ctrl+K to start a new chat, ",
                "Escape to cancel streaming, ",
                "or Ctrl+/ to see all shortcuts."
            ],
            ["features"] = [
                "This demo showcases many features: ",
                "1. Multiple agents (try switching!) ",
                "2. Conversation history ",
                "3. Keyboard shortcuts ",
                "4. Event logging ",
                "5. State viewer ",
                "6. Mobile responsive design"
            ]
        },
        ["coder"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["hello"] = ["Hey developer! ", "Ready to write some code?"],
            ["blazor"] = [
                "Blazor is a fantastic framework! ",
                "This app uses Blazor Server with SignalR. ",
                "The real-time communication gives us instant streaming responses."
            ],
            ["code"] = [
                "Here's a simple example:\n",
                "```csharp\n",
                "public class Example\n",
                "{\n",
                "    public string Message => \"Hello, Blazor!\";\n",
                "}\n",
                "```\n",
                "Let me know if you need help with anything specific!"
            ]
        },
        ["researcher"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["hello"] = ["Hello researcher! ", "What topic shall we investigate?"],
            ["ai"] = [
                "AI has made tremendous progress recently. ",
                "Large Language Models like GPT-4 and Claude ",
                "have revolutionized how we interact with AI. ",
                "This demo uses Microsoft.Extensions.AI interfaces."
            ]
        }
    };

    public MockChatClient(string agentType = "assistant")
    {
        _agentType = agentType;
    }

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
        providerName: $"MockChatClient-{_agentType}",
        providerUri: new Uri("https://github.com/lionfire/ag-ui-blazor"),
        modelId: $"demo-{_agentType}-v1");

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
        // Get agent-specific responses
        if (_contextualResponses.TryGetValue(_agentType, out var agentResponses))
        {
            foreach (var (keyword, responses) in agentResponses)
            {
                if (userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return responses;
                }
            }
        }

        // Return default intro for this agent type
        return _agentIntros.TryGetValue(_agentType, out var intro) ? intro : _agentIntros["assistant"];
    }
}
