---
greenlit: true
implementationDone: true
completedDate: 2025-12-11
---

# Epic 01-005: DirectAgentClientFactory Implementation

**Phase**: 01 - MudBlazor MVP
**Status**: In Progress
**Estimated Effort**: 3-4 days

## Overview

Implement `DirectAgentClientFactory` that provides direct streaming from agent to component without HTTP overhead, achieving sub-millisecond latency.

**Link to Phase**: [Phase 01: MudBlazor MVP](../../40-phases/01-mudblazor-mvp.md)

## Status Overview

- [x] Factory Implementation (Core)
- [x] Service Registration
- [ ] Performance Optimization (Deferred - requires actual agent usage to profile)
- [x] Testing (Unit tests passing - 52 tests)

## Implementation Tasks

### Factory Implementation
- [x] Create `DirectAgentClientFactory.cs` in Server package
- [x] Implement `IAgentClientFactory` interface
- [x] Inject `IServiceProvider` for agent lookup
- [x] Implement `GetAgentAsync`:
  - [x] Lookup agent by name using keyed services
  - [x] Cache agent references (agents are registered as singletons)
  - [x] Return null if not found (logs warning with available agents)
- [x] Implement `ListAgentsAsync`: return registered agent names
- [x] Implement `GetConnectionState`: always return Connected

### Service Registration
- [x] Create `ServiceCollectionExtensions.cs` for Server
- [x] Implement `AddAgUiBlazorServer()` method:
  - [x] Register DirectAgentClientFactory as singleton
  - [x] Register IAgentClientFactory
  - [x] Configure options system
- [x] Implement `AddAgent()` extension:
  - [x] Register agent with keyed service
  - [x] Support `Func<IServiceProvider, IChatClient>` factory
  - [x] Support direct instance registration
  - [x] Support generic type registration
  - [x] Validate agent name (not null/empty)

### Configuration
- [x] Create `AgentRegistryOptions.cs` for agent registry configuration
- [x] Implement `RegisterAgent` method with validation
- [x] Case-sensitive agent name storage

### Performance Optimization
- [ ] Profile streaming performance
- [ ] Measure first token latency
- [ ] Optimize hot path
- [ ] Verify < 1ms target achieved
- [ ] Benchmark against HTTP approach

### Testing
- [x] Unit tests for factory (DirectAgentClientFactoryTests.cs)
- [x] Test agent registration (AgentRegistryOptionsTests.cs)
- [x] Test agent lookup (found/not found)
- [x] Test DI integration (ServiceCollectionExtensionsTests.cs)
- [x] Integration test with mock agent
- [ ] Performance benchmark tests

## Acceptance Criteria

- [x] Factory returns agents directly from DI
- [x] No HTTP overhead in streaming (direct in-process access)
- [ ] < 1ms first token latency measured (requires benchmark with actual agent)
- [x] Agent registration is simple and clear
- [x] Helpful error messages when agent not found
- [x] Tests pass (52 tests passing)
- [ ] Performance benchmarks document latency

## Files Created

### Source Files
- `/src/ag-ui-blazor/src/LionFire.AgUi.Blazor.Server/Services/DirectAgentClientFactory.cs`
- `/src/ag-ui-blazor/src/LionFire.AgUi.Blazor.Server/Extensions/ServiceCollectionExtensions.cs`
- `/src/ag-ui-blazor/src/LionFire.AgUi.Blazor.Server/Configuration/AgentRegistryOptions.cs`

### Test Files
- `/src/ag-ui-blazor/tests/LionFire.AgUi.Blazor.Server.Tests/Services/DirectAgentClientFactoryTests.cs`
- `/src/ag-ui-blazor/tests/LionFire.AgUi.Blazor.Server.Tests/Extensions/ServiceCollectionExtensionsTests.cs`
- `/src/ag-ui-blazor/tests/LionFire.AgUi.Blazor.Server.Tests/Configuration/AgentRegistryOptionsTests.cs`

## Usage Example

```csharp
// In Program.cs
builder.Services
    .AddAgUiBlazorServer()
    .AddAgent<MyAssistant>("assistant", "A helpful AI assistant")
    .AddAgent("chatbot", sp => new ChatBotAgent(sp.GetRequiredService<IConfiguration>()));

// In a Blazor component
@inject IAgentClientFactory AgentFactory

var agent = await AgentFactory.GetAgentAsync("assistant");
if (agent != null)
{
    await foreach (var update in agent.CompleteStreamingAsync(messages))
    {
        // Handle streaming response
    }
}
```
