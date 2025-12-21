# Microsoft.Extensions.AI

## Brief Description

Microsoft.Extensions.AI provides unified abstractions for AI services, primarily IChatClient for chat completions, enabling provider-agnostic AI integration.

## Relevance to Project

**Why this matters**:
- IChatClient is the universal agent interface
- Works with OpenAI, Anthropic, Ollama, Azure, etc.
- Enables middleware (logging, retry, caching)
- Our components work with any IChatClient implementation

**Where it's used**:
- Agent registration: Agents implement or wrap IChatClient
- DirectAgentClientFactory: Returns AIAgent wrapping IChatClient
- Sample applications: Use IChatClient for mock agents

## Interoperability Points

**Core types**:
- IChatClient: Main interface
- ChatMessage: Message format (User, Assistant, System roles)
- ChatCompletion: Response type
- FunctionCallContent: Tool calling
- ChatOptions: Model configuration

## Considerations

### Best Practices

- Use IChatClient for all agent interactions
- Leverage middleware pipeline (logging, retry)
- Use standard message types (ChatMessage)
- Follow async patterns throughout

### Common Pitfalls

- Mixing message types: Stick to Microsoft.Extensions.AI types
- Not handling streaming: CompleteStreamingAsync returns IAsyncEnumerable

## References

- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/ai/conceptual/ai-extensions-overview)
- [IChatClient Introduction](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.ichatclient)
