# AG-UI Protocol

## Brief Description

AG-UI (Agent UI) is Microsoft's protocol for standardized agent-to-UI communication, enabling real-time streaming, tool calling, and state synchronization between AI agents and user interfaces.

## Relevance to Project

**Why this matters**:
- Foundation of agent communication - we don't reimplement this
- Defines event types and message formats
- Enables interoperability with any AG-UI compatible agent
- Provides SSE streaming and thread management

**Where it's used**:
- Microsoft.Agents.AI.Hosting.AGUI.AspNetCore: Server-side AG-UI
- Microsoft.Agents.AI.AGUI: Client-side AG-UI (WASM)
- MapAGUI() endpoint setup
- AGUIChatClient for HTTP/SSE consumption

## Interoperability Points

**Integrates with**:
- Microsoft.Extensions.AI: IChatClient abstraction
- Microsoft Agent Framework: AIAgent wrapper
- SSE (Server-Sent Events): Streaming transport
- HTTP: Request-response transport

**Event types**:
- TextContent: Streamed text chunks
- FunctionCallContent: Tool call requests
- FunctionResultContent: Tool results
- ThreadStatus: Conversation state updates

## Considerations

### Best Practices

- Use Microsoft's AG-UI packages (don't rebuild protocol)
- Follow AG-UI event patterns
- Map events to UI appropriately
- Handle all event types gracefully

### Common Pitfalls

- Assuming only text events: Tool calls and other events also occur
- Not handling connection errors: SSE can disconnect
- Blocking on synchronous calls: Use async throughout

### Performance

- SSE streaming is efficient for real-time updates
- Direct streaming (Server) bypasses HTTP for better performance
- HTTP/SSE (WASM) adds ~20-50ms latency vs direct

## References

- [AG-UI Documentation](https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/)
- [AG-UI Getting Started](https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/getting-started)
