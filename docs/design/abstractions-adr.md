# ADR-001: Core Abstractions Design

**Status**: Accepted
**Date**: 2024-12-11
**Authors**: Engineering Team

## Context

The AG-UI Blazor library needs to support both Blazor Server and Blazor WebAssembly (WASM) hosting models while providing a consistent API for interacting with AI agents. The key challenges are:

1. **Different Runtime Environments**: Blazor Server runs on the server with direct access to backend services, while Blazor WASM runs entirely in the browser.
2. **Connection Management**: Server can maintain direct connections to AI services; WASM must proxy through HTTP/SignalR.
3. **State Persistence**: Server can use server-side storage; WASM needs browser-based storage with optional sync.
4. **Security Considerations**: Tool approval and authentication differ significantly between hosting models.

## Decision

We will create a set of core abstractions (interfaces) that hide the implementation differences between hosting models:

### Core Interfaces

1. **IAgentClientFactory** - Abstracts agent creation and discovery
2. **IAgentStateManager** - Abstracts conversation persistence
3. **IToolApprovalService** - Abstracts tool call approval workflows

### Design Principles Applied

1. **Interface Segregation Principle (ISP)**: Each interface has a focused responsibility
2. **Dependency Inversion Principle (DIP)**: Components depend on abstractions, not implementations
3. **Immutability**: Models use records with `init` accessors where practical
4. **Async-First**: All I/O operations are async with `CancellationToken` support

## Alternatives Considered

### Alternative 1: Single Unified Implementation with Configuration

**Approach**: One implementation that switches behavior based on runtime detection.

**Rejected Because**:
- Creates a "god class" with complex conditional logic
- Harder to test different scenarios in isolation
- Violates Single Responsibility Principle

### Alternative 2: Direct Dependency on Microsoft.Agents.AI

**Approach**: Directly use `AIAgent` and `AgentThread` throughout the codebase.

**Rejected Because**:
- Tightly couples to a specific AI SDK
- Makes testing difficult without actual AI service
- WASM cannot directly reference server-side SDK types

### Alternative 3: Event-Based Architecture Only

**Approach**: Use only events/observables for all agent interactions.

**Rejected Because**:
- Increases complexity for simple request-response patterns
- Harder for developers familiar with async/await
- Still needs abstractions for event source implementations

## Consequences

### Positive

1. **Testability**: Interfaces can be easily mocked for unit testing
2. **Flexibility**: New implementations can be added without changing consuming code
3. **Clarity**: Clear separation between Blazor Server and WASM implementations
4. **Maintainability**: Changes to one hosting model don't affect the other

### Negative

1. **Indirection**: Additional layer between components and actual services
2. **Boilerplate**: Need to implement interfaces for each hosting model
3. **Learning Curve**: Developers must understand the abstraction layer

### Neutral

1. **Performance**: Minimal overhead from interface dispatch
2. **Complexity**: Complexity is shifted to implementation, not increased overall

## Implementation Details

### Models

All data transfer objects are implemented as C# records for:
- Immutability (thread-safety)
- Value equality
- Concise syntax
- Built-in `with` expressions for modifications

Exception: `ChatViewModel` is a mutable class for Blazor component state binding.

### Interface Design

- Return `Task<T?>` (nullable) for "get" operations that may not find results
- Return `Task` for void operations
- All async methods accept `CancellationToken ct = default`
- Use `IReadOnlyList<T>` for collection returns (not `IEnumerable<T>`) to indicate finite, enumerated results

### Type References

- Use `Microsoft.Extensions.AI.ChatMessage` for message types (standard .NET abstraction)
- Do NOT reference `Microsoft.Agents.AI` types directly (implementation detail)
- Keep abstractions independent of specific AI SDK implementations

## Related Decisions

- ADR-002 (planned): Event sourcing for real-time updates
- ADR-003 (planned): Authentication and authorization model

## References

- [AG-UI Protocol Specification](https://github.com/ag-ui-protocol/ag-ui)
- [Blazor Hosting Models](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models)
- [Microsoft.Extensions.AI](https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.AI.Abstractions)
