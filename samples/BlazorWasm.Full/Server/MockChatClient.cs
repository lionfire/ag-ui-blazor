using Microsoft.Extensions.AI;

namespace BlazorWasm.Full.Server;

/// <summary>
/// A mock chat client that simulates AI responses with different personalities.
/// </summary>
public class MockChatClient : IChatClient
{
    private readonly string _agentType;
    private static readonly Random _random = new();

    public MockChatClient(string agentType)
    {
        _agentType = agentType;
    }

    public ChatClientMetadata Metadata => new("MockChatClient", new Uri("https://example.com"), $"mock-{_agentType}-v1");

    public async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(_random.Next(100, 500), cancellationToken);

        var lastUserMessage = chatMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";
        var response = GenerateResponse(lastUserMessage);

        return new ChatCompletion(new ChatMessage(ChatRole.Assistant, response))
        {
            CompletionId = Guid.NewGuid().ToString(),
            ModelId = $"mock-{_agentType}-v1"
        };
    }

    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastUserMessage = chatMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";
        var response = GenerateResponse(lastUserMessage);

        // Simulate streaming by yielding words
        var words = response.Split(' ');
        foreach (var word in words)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            await Task.Delay(_random.Next(30, 80), cancellationToken);
            yield return new StreamingChatCompletionUpdate
            {
                Text = word + " "
            };
        }
    }

    private string GenerateResponse(string userMessage)
    {
        var lowerMessage = userMessage.ToLowerInvariant();

        return _agentType switch
        {
            "assistant" => GenerateAssistantResponse(lowerMessage),
            "coder" => GenerateCoderResponse(lowerMessage),
            "researcher" => GenerateResearcherResponse(lowerMessage),
            _ => $"I received your message: \"{userMessage}\". How can I help you today?"
        };
    }

    private static string GenerateAssistantResponse(string message)
    {
        if (message.Contains("hello") || message.Contains("hi"))
            return "Hello! I'm your AI assistant. I'm here to help you with any questions or tasks you might have. What would you like to discuss today?";

        if (message.Contains("help"))
            return "I'd be happy to help! I can assist you with a wide variety of tasks including answering questions, providing explanations, helping with writing, and much more. Just let me know what you need!";

        if (message.Contains("offline"))
            return "This demo supports offline mode! When you're offline, your messages are queued locally and will be sent automatically when the connection is restored. Try disconnecting from the network to see it in action.";

        if (message.Contains("wasm") || message.Contains("webassembly"))
            return "This application runs entirely in your browser using WebAssembly (WASM). It means the Blazor code is compiled to run client-side, providing a responsive experience even with slow network connections.";

        return $"Thank you for your message! As your AI assistant, I'm designed to be helpful, harmless, and honest. I can help you with questions, explanations, writing tasks, and much more. What would you like to explore?";
    }

    private static string GenerateCoderResponse(string message)
    {
        if (message.Contains("hello") || message.Contains("hi"))
            return "Hey there, fellow developer! I'm your coding assistant. I specialize in programming, software architecture, and technical problem-solving. What are you working on today?";

        if (message.Contains("blazor"))
            return "Blazor is a fantastic framework for building interactive web UIs using C# instead of JavaScript. This very application is built with Blazor WebAssembly! The component model is similar to other popular frameworks, but with the power of .NET behind it.";

        if (message.Contains("code") || message.Contains("function") || message.Contains("class"))
            return "I'd love to help with your code! Whether you need help writing new functions, debugging existing code, or designing class structures, I'm here to assist. Share your code or describe what you're trying to accomplish.";

        if (message.Contains("error") || message.Contains("bug"))
            return "Debugging is a crucial skill! When approaching bugs, I recommend: 1) Reproduce the issue consistently, 2) Check the error message and stack trace, 3) Isolate the problematic code, 4) Add logging or breakpoints, 5) Test your fix thoroughly. What error are you seeing?";

        return "As a coding assistant, I can help with code reviews, debugging, architecture discussions, and learning new programming concepts. I'm familiar with many languages and frameworks. What would you like to work on?";
    }

    private static string GenerateResearcherResponse(string message)
    {
        if (message.Contains("hello") || message.Contains("hi"))
            return "Greetings! I'm your research assistant. I specialize in analysis, information synthesis, and helping you explore topics in depth. What subject would you like to investigate today?";

        if (message.Contains("research") || message.Contains("study"))
            return "Research methodology is crucial for reliable findings. I recommend starting with a clear research question, reviewing existing literature, defining your methodology, collecting data systematically, and analyzing results objectively. What topic are you researching?";

        if (message.Contains("data") || message.Contains("analysis"))
            return "Data analysis involves several key steps: data collection, cleaning, exploration, analysis, and interpretation. The choice of analytical methods depends on your data type and research questions. Would you like to discuss a specific analysis approach?";

        if (message.Contains("source") || message.Contains("reference"))
            return "Evaluating sources is critical for good research. Consider the author's credentials, publication venue, methodology, citations, and potential biases. Primary sources provide firsthand evidence, while secondary sources analyze and interpret. What sources are you evaluating?";

        return "As a research assistant, I can help you explore topics thoroughly, analyze information, evaluate sources, and synthesize findings. I aim to provide balanced, well-reasoned perspectives. What area would you like to investigate?";
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }
}
