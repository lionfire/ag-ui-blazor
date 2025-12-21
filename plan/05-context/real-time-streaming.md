# Real-Time Streaming

## Brief Description

Real-time streaming enables token-by-token display of AI responses using IAsyncEnumerable and StateHasChanged, providing immediate user feedback as content generates.

## Relevance to Project

**Why this matters**:
- Core feature: Users see responses as they're generated
- Performance critical: Blazor Server achieves < 1ms latency
- UX differentiator: Much better than waiting for complete response

**Where it's used**:
- DirectAgentClientFactory: Direct IAsyncEnumerable streaming
- HttpAgentClientFactory: SSE streaming over HTTP
- MudMessageList: Displays streaming updates

## Considerations

### Best Practices

**For Blazor Server (Direct Streaming)**:
- Use IAsyncEnumerable<T> directly from agent
- Call StateHasChanged() strategically (every N tokens)
- Profile latency to ensure < 1ms target
- Avoid HTTP serialization

**For WASM (HTTP/SSE Streaming)**:
- Use SSE for server-sent events
- Handle reconnection gracefully
- Buffer updates client-side if needed
- Target < 50ms latency

### Performance Considerations

- Don't call StateHasChanged() on every character (too expensive)
- Batch updates (e.g., every 10 characters or 16ms)
- Use string concatenation carefully (StringBuilder for large content)
- Profile render performance with large messages

### Common Pitfalls

- Calling StateHasChanged() too frequently: Degrades performance
- Not handling cancellation: Streams can't be stopped
- Memory leaks from unclosed streams: Always dispose

## References

- [IAsyncEnumerable in C#](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios)
- [SSE in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
