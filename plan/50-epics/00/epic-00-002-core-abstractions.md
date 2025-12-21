---
greenlit: true
implementationDone: true
completedDate: 2025-12-11
---

# Epic 00-002: Core Abstractions Design

**Phase**: 00 - Foundation
**Status**: Complete
**Estimated Effort**: 3-4 days

## Overview

Design and implement core abstractions (interfaces) that enable both Blazor Server and WASM implementations while maintaining clean separation of concerns and extensibility.

**Link to Phase**: [Phase 00: Foundation](../../40-phases/00-foundation.md)

## Status Overview

- [x] Planning complete
- [x] Abstractions designed and reviewed
- [x] Interfaces implemented
- [x] XML documentation complete
- [x] Unit tests written
- [ ] Code review complete

## Implementation Tasks

### Design Review
- [x] Review Microsoft.Agents.AI abstractions
  - [x] Study `IChatClient` interface
  - [x] Study `AIAgent` class
  - [x] Study `AgentThread` class
  - [x] Study `AgentRunResponseUpdate` structure
  - [x] Understand AG-UI event types

- [x] Define abstraction requirements
  - [x] List Server-specific requirements
  - [x] List WASM-specific requirements
  - [x] Identify common functionality
  - [x] Determine extension points

- [x] Create abstraction design document
  - [x] Document interface purposes
  - [x] Document method signatures
  - [x] Document extension patterns
  - [ ] Get design approval from team

### IAgentClientFactory Interface
- [x] Create `IAgentClientFactory.cs` in `src/LionFire.AgUi.Blazor/Abstractions/`
  - [x] Add `Task<IChatClient?> GetAgentAsync(string agentName, CancellationToken ct = default)`
  - [x] Add `Task<IReadOnlyList<AgentInfo>> ListAgentsAsync(CancellationToken ct = default)`
  - [x] Add `ConnectionState GetConnectionState()`
  - [x] Add comprehensive XML documentation
  - [x] Document Server vs WASM behavior differences

- [x] Create `AgentInfo.cs` model
  - [x] Add `string Name { get; init; }`
  - [x] Add `string? Description { get; init; }`
  - [x] Add `string? IconUrl { get; init; }`
  - [x] Add `bool IsAvailable { get; init; }`
  - [x] Make it a record for immutability

- [x] Create `ConnectionState` enum
  - [x] Add `Connected` state
  - [x] Add `Connecting` state
  - [x] Add `Disconnected` state
  - [x] Add `Reconnecting` state
  - [x] Add `Error` state
  - [x] Add XML documentation for each state

### IAgentStateManager Interface
- [x] Create `IAgentStateManager.cs` in `src/LionFire.AgUi.Blazor/Abstractions/`
  - [x] Add `Task SaveConversationAsync(Conversation conversation, CancellationToken ct = default)`
  - [x] Add `Task<Conversation?> LoadConversationAsync(string conversationId, CancellationToken ct = default)`
  - [x] Add `Task<IReadOnlyList<ConversationMetadata>> ListConversationsAsync(CancellationToken ct = default)`
  - [x] Add `Task DeleteConversationAsync(string conversationId, CancellationToken ct = default)`
  - [x] Add comprehensive XML documentation

- [x] Create `Conversation.cs` model
  - [x] Add `string Id { get; init; }`
  - [x] Add `string AgentName { get; init; }`
  - [x] Add `IReadOnlyList<ChatMessage> Messages { get; init; }`
  - [x] Add `DateTimeOffset CreatedAt { get; init; }`
  - [x] Add `DateTimeOffset LastModifiedAt { get; init; }`
  - [x] Add `IReadOnlyDictionary<string, object>? Metadata { get; init; }`

- [x] Create `ConversationMetadata.cs` model
  - [x] Add `string Id { get; init; }`
  - [x] Add `string AgentName { get; init; }`
  - [x] Add `string? Title { get; init; }`
  - [x] Add `DateTimeOffset CreatedAt { get; init; }`
  - [x] Add `DateTimeOffset LastModifiedAt { get; init; }`
  - [x] Add `int MessageCount { get; init; }`
  - [x] Add `IReadOnlyList<string> Tags { get; init; }`

### IToolApprovalService Interface
- [x] Create `IToolApprovalService.cs` in `src/LionFire.AgUi.Blazor/Abstractions/`
  - [x] Add `Task<ToolApprovalResult> RequestApprovalAsync(ToolCall toolCall, CancellationToken ct = default)`
  - [x] Add `Task<bool> ShouldApproveAutomaticallyAsync(ToolCall toolCall, CancellationToken ct = default)`
  - [x] Add `ToolApprovalMode ApprovalMode { get; }`
  - [x] Add XML documentation

- [x] Create `ToolCall.cs` model
  - [x] Add `string Id { get; init; }`
  - [x] Add `string Name { get; init; }`
  - [x] Add `string? Description { get; init; }`
  - [x] Add `IReadOnlyDictionary<string, object>? Arguments { get; init; }`
  - [x] Add `ToolRiskLevel RiskLevel { get; init; }`
  - [x] Add `DateTimeOffset RequestedAt { get; init; }`

- [x] Create `ToolApprovalResult.cs` model
  - [x] Add `bool IsApproved { get; init; }`
  - [x] Add `string? DenialReason { get; init; }`
  - [x] Add `IReadOnlyDictionary<string, object>? ModifiedArguments { get; init; }`
  - [x] Add `DateTimeOffset RespondedAt { get; init; }`

- [x] Create `ToolApprovalMode` enum
  - [x] Add `Blocking` mode
  - [x] Add `Async` mode
  - [x] Add `AutoApprove` mode

- [x] Create `ToolRiskLevel` enum
  - [x] Add `Safe` level
  - [x] Add `Risky` level
  - [x] Add `Dangerous` level

### Shared Models
- [x] Create `ChatViewModel.cs` in `src/LionFire.AgUi.Blazor/Models/`
  - [x] Add `string ConversationId { get; set; }`
  - [x] Add `List<ChatMessage> Messages { get; set; }`
  - [x] Add `bool IsStreaming { get; set; }`
  - [x] Add `ConnectionState ConnectionState { get; set; }`
  - [x] Add `List<ToolCall> PendingToolCalls { get; set; }`
  - [x] Add `string? ErrorMessage { get; set; }`

- [x] Create `TokenUsage.cs` model
  - [x] Add `int PromptTokens { get; init; }`
  - [x] Add `int CompletionTokens { get; init; }`
  - [x] Add `int TotalTokens { get; init; }`
  - [x] Add `decimal? EstimatedCost { get; init; }`
  - [x] Add `string? ModelName { get; init; }`

### Extension Methods
- [x] Create `AgentClientFactoryExtensions.cs`
  - [x] Add `Task<IChatClient> GetOrThrowAsync(this IAgentClientFactory factory, string name)` with helpful error
  - [x] Add `Task<bool> IsAgentAvailableAsync(this IAgentClientFactory factory, string name)`

### Documentation
- [x] Document abstraction design decisions
  - [x] Create ADR (Architecture Decision Record) for abstractions
  - [x] Explain why these abstractions vs alternatives
  - [x] Document extension patterns

- [x] Add code examples
  - [x] Create example implementation for Server (in XML docs)
  - [x] Create example implementation for WASM (in XML docs)
  - [x] Add to XML docs as remarks

### Unit Tests
- [x] Create abstraction tests
  - [x] Test that interfaces can be mocked
  - [x] Test model serialization (JSON)
  - [x] Test model validation
  - [x] Test enum values

- [x] Create extension method tests
  - [x] Test `GetOrThrowAsync` throws with clear message
  - [x] Test `IsAgentAvailableAsync` handles nulls

## Dependencies & Blockers

**Upstream Dependencies**:
- Epic 00-001 (needs project structure)

**Blocks**:
- All Phase 1 implementation epics (need abstractions)

## Acceptance Criteria

- [x] All interfaces have clear, single responsibilities
- [x] All public APIs have comprehensive XML documentation
- [x] Models are immutable where appropriate (records or init-only properties)
- [x] Enums have XML docs for each value
- [x] Abstractions work for both Server and WASM scenarios
- [x] No implementation details leak into abstractions
- [x] Extension methods provide helpful utilities
- [ ] Design review approved by at least 2 developers
- [x] Unit tests achieve > 90% code coverage for models
- [x] ADR document explains design rationale

## Notes

**Design Principles**:
- Keep interfaces minimal (ISP - Interface Segregation Principle)
- Favor composition over inheritance
- Use cancellation tokens for async operations
- Return Task for all async operations (not ValueTask in public API)
- Use nullable reference types correctly
- Make models immutable when possible

**Naming Conventions**:
- Interfaces: `IServiceName`
- Models: `NounName` (no suffix)
- Enums: `AdjectiveName` or `NounName`
- Avoid "Manager" suffix unless truly managing resources

**Anti-Patterns to Avoid**:
- God interfaces (too many methods)
- Leaky abstractions (exposing implementation details)
- Anemic models (models with no behavior when behavior makes sense)
- Mutable shared state in models

## Implementation Notes

### Files Created

**Abstractions** (`src/LionFire.AgUi.Blazor/Abstractions/`):
- `IAgentClientFactory.cs` - Factory for creating/retrieving agents
- `IAgentStateManager.cs` - Conversation persistence management
- `IToolApprovalService.cs` - Tool call approval workflow

**Models** (`src/LionFire.AgUi.Blazor/Models/`):
- `AgentInfo.cs` - Agent metadata record
- `ConnectionState.cs` - Connection state enum
- `Conversation.cs` - Full conversation record
- `ConversationMetadata.cs` - Lightweight conversation metadata
- `ToolCall.cs` - Tool call request record
- `ToolApprovalResult.cs` - Tool approval response record
- `ToolApprovalMode.cs` - Approval mode enum
- `ToolRiskLevel.cs` - Tool risk level enum
- `ChatViewModel.cs` - Mutable view model for Blazor components
- `TokenUsage.cs` - Token usage statistics record

**Extensions** (`src/LionFire.AgUi.Blazor/Extensions/`):
- `AgentClientFactoryExtensions.cs` - Helper extension methods

**Documentation** (`docs/design/`):
- `abstractions-adr.md` - Architecture Decision Record

**Tests** (`tests/LionFire.AgUi.Blazor.Tests/`):
- `Models/EnumTests.cs`
- `Models/AgentInfoTests.cs`
- `Models/ConversationTests.cs`
- `Models/ConversationMetadataTests.cs`
- `Models/ToolCallTests.cs`
- `Models/ToolApprovalResultTests.cs`
- `Models/TokenUsageTests.cs`
- `Models/ChatViewModelTests.cs`
- `Abstractions/InterfaceMockingTests.cs`
- `Extensions/AgentClientFactoryExtensionsTests.cs`

### Build Results

- **Build**: Succeeded with 0 warnings
- **Tests**: 92 tests passing (net8.0 and net9.0)
