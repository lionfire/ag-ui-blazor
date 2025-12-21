# Blazor AG-UI Components - Clarified Product Requirements Prompt

**Version**: 2.0 (Clarified)
**Date**: 2025-12-11
**Based On**: Original PRP.md
**Status**: Active Development

## Overview

LionFire.AgUi.Blazor provides production-ready MudBlazor components for integrating AI agents into Blazor applications using Microsoft's AG-UI protocol. This library builds on Microsoft's solid foundation (Microsoft.Extensions.AI, Agent Framework, AG-UI packages) rather than reimplementing the protocol, focusing specifically on the Blazor UI layer with beautiful, rich MudBlazor components.

The key differentiator is Blazor Server optimization through direct streaming that bypasses HTTP entirely, achieving 10-50x better performance than HTTP-based approaches (< 1ms latency vs 50ms+).

## Problem Statement

Blazor developers currently lack pre-built, production-ready UI components for integrating AI agents using the Microsoft Agent Framework's AG-UI protocol. While Microsoft provides the protocol implementation and server/client libraries, developers must build their own UI layer from scratch. This creates a significant barrier to adoption and results in duplicated effort across projects. Additionally, Blazor Server has the potential for much better performance than HTTP-based approaches, but this optimization requires specialized knowledge that most developers don't have.

## Target Audiences

### Primary Audiences

1. **Blazor Server Developers**: .NET developers building Interactive Server applications who need maximum performance with real-time AI streaming and simple integration (< 5 minutes setup)

2. **Blazor WASM Developers**: .NET developers building WebAssembly applications who need offline support, automatic reconnection, and reliable behavior on unreliable mobile networks

### Secondary Audiences

3. **Hybrid App Developers**: Developers building applications that support both Blazor Server and WASM rendering modes, requiring shared components and mode-specific optimizations

4. **Library/Framework Authors**: Developers building higher-level frameworks (like Axi) who need extensible, well-documented components with stable public interfaces

### Tertiary Audiences

5. **.NET Developers New to Blazor**: ASP.NET, WPF, or WinForms developers exploring Blazor who want to add AI capabilities with minimal Blazor-specific knowledge

6. **AI/ML Engineers**: AI/ML engineers who need to build UIs for their models but aren't primarily frontend developers

## Core Requirements

### Functional Requirements

#### MudBlazor Components (PRIMARY FOCUS)

**Package Structure**:
- `LionFire.AgUi.Blazor` - Minimal base package with abstractions and shared logic (basic HTML/CSS only if needed)
- `LionFire.AgUi.Blazor.MudBlazor` - Rich, full-featured MudBlazor components (PRIMARY PACKAGE)
- `LionFire.AgUi.Blazor.Server` - Blazor Server optimizations (direct streaming)
- `LionFire.AgUi.Blazor.Wasm` - WASM client with offline support
- `LionFire.AgUi.Blazor.Testing` - Test utilities and mocks

**Core Components**:
- `MudAgentChat` - Main chat UI component with MudBlazor styling
- `MudMessageList` - Message display with virtual scrolling (MudVirtualize)
- `MudMessageInput` - Rich input with MudTextField
- `MudToolCallPanel` - Tool execution UI with MudDialog for approvals
- `MudConnectionStatus` - Connection indicator using MudChip
- `MudAgentSelector` - Multi-agent switcher with MudSelect or MudMenu
- `MudEventLog` - Debug event viewer with MudDataGrid
- `MudStateViewer` - State visualization with MudExpansionPanel

**Theme Integration**:
- Full integration with MudBlazor theming system
- Support for light/dark mode switching
- Custom theme support via MudTheme
- Respect app-wide MudBlazor configuration

#### Message Rendering

**Markdown Support**:
- Render agent messages with proper Markdown formatting
- Support code blocks, lists, links, tables, images
- Syntax highlighting for code blocks (integrate highlight.js or Prism)
- Copy-to-clipboard button for code blocks
- LaTeX/math rendering for technical content (optional)

**Content Types**:
- Text messages with Markdown
- Code blocks with language detection
- Tool call displays (pending, approved, denied)
- Error messages with actionable guidance
- System messages with different styling
- Streaming indicators during generation

#### Conversation Management

**History Management**:
- View list of past conversations
- Search within conversation history
- Delete or archive conversations
- Conversation tagging/categorization
- Conversation export (JSON, Markdown, PDF)
- Conversation import for restoration

**Message Operations**:
- Regenerate last agent response
- Stop generation mid-stream
- Edit previous user messages and resend
- Message timestamps
- Message bookmarking for quick reference
- Branch conversation from any point (future)

#### Blazor Server Direct Streaming

**Performance Optimization**:
- `DirectAgentClientFactory` bypasses HTTP entirely
- Direct `IAsyncEnumerable<AgentRunResponseUpdate>` streaming
- Zero serialization/deserialization overhead
- Target < 1ms first token latency
- SignalR handles browser sync automatically

**State Management**:
- In-memory state by default
- Optional distributed cache (Redis, SQL) via `IAgentStateManager`
- Conversation persistence across reconnections
- Graceful handling of circuit reconnections

#### Blazor WASM Offline Support

**Network Handling**:
- `ConnectionMonitor` detects offline/online state
- `OfflineAgentClient` queues messages during disconnection
- Exponential backoff reconnection strategy
- Request queue with configurable size (500+ messages)
- Connection status UI with reconnection feedback

**State Management**:
- LocalStorage for conversation persistence
- Optional IndexedDB for large conversations
- Offline queue status visualization
- Sync strategy when reconnecting

#### Tool Approval (Human-in-the-Loop)

**Approval Modes**:
- **Blocking mode** (default): MudDialog modal blocks until approval/deny
- **Async mode** (optional): MudSnackbar notifications, non-blocking queue UI

**Tool UI**:
- Display tool name, description, arguments
- Show risk level (safe, risky, dangerous)
- Approval/deny buttons
- Tool execution progress
- Tool result display
- Audit trail of approvals

#### Agent Management

**Multi-Agent Support** (Phase 2+):
- Single agent per conversation in Phase 1
- `MudAgentSelector` to switch between agents
- New thread per agent in Phase 1
- Multi-agent conversations in Phase 2+
- Agent handoff patterns
- Visual distinction (avatars, colors)

**Agent Configuration**:
- Register agents via `AddAgent(name, factory)`
- Support for any `IChatClient` implementation
- Custom agent avatars/icons
- Agent status indicators (thinking, idle, error)
- Agent metadata display

#### Authentication Integration

**Approach**: Authentication-agnostic at component level

**Documentation/Samples for**:
- JWT bearer tokens for WASM clients
- Cookie authentication for Server
- Azure AD / OAuth integration
- Custom authentication headers
- `AuthenticationMessageHandler` support

#### Error Handling and Retry

**Retry Policy** (configurable):
- Transient errors: Auto-retry with exponential backoff (max 3 attempts)
- Rate limits: Auto-retry after rate limit reset time
- Authentication errors: No retry, show error to user
- Server errors (500): One retry, then show error
- Provide `RetryPolicy` configuration option

**Error Display**:
- User-friendly error messages
- Actionable guidance (e.g., "API key invalid - check configuration")
- Error recovery suggestions
- Debug information for developers

#### Service Registration

**Blazor Server**:
```csharp
builder.Services.AddAgUiBlazorServer()
    .AddAgent("name", sp => { /* create agent */ });
```

**Blazor WASM**:
```csharp
builder.Services.AddAgUiBlazorWasm(options => {
    options.ServerUrl = "...";
    options.EnableOfflineMode = true;
});
```

### Non-Functional Requirements

#### Performance

| Metric | Target | Priority |
|--------|--------|----------|
| Blazor Server first token latency | < 1ms | High |
| Blazor WASM first token latency | < 50ms | High |
| WASM reconnection time | < 2s | High |
| WASM bundle size overhead | < 50KB (base) | High |
| Memory per connection (Server) | < 5MB | Medium |
| Event throughput (Server) | > 10k/sec | Medium |

#### Streaming Performance

**Phase 1**: Full message buffering with warning if message exceeds 100KB

**Phase 2+**: Optional virtualization
- MudVirtualize for message lists
- Configurable max message size
- Option to truncate old messages
- `EnableVirtualization` and `MaxMessageSize` parameters

#### Platform Support

| Requirement | Target | Priority |
|-------------|--------|----------|
| .NET versions | net8.0, net9.0 | High |
| Browser support | Edge, Chrome, Firefox, Safari | High |
| Mobile responsive | All components | High |
| Touch-friendly | WASM components | Medium |
| Accessibility | WCAG 2.1 AA | Medium |

#### Developer Experience

**Getting Started**:
- From `dotnet new` to working chat in < 5 minutes
- Samples run out-of-the-box (no configuration)
- Clear error messages with actionable guidance
- Component docs with live examples

**Documentation**:
- Quick start guide
- Blazor Server specific guide
- Blazor WASM specific guide
- Component reference
- Performance optimization guide
- Migration guide from raw SignalR/AG-UI

#### Testing Support

**Testing Package** (`LionFire.AgUi.Blazor.Testing`):
- `FakeAgentClientFactory` with mock agents
- `MockAIAgent` with predictable responses
- Test helpers for component testing
- bUnit integration examples

## Extended Features

### High Priority (Phase 1-2)

**Must-Have**:
- Message Markdown rendering with syntax highlighting
- Code syntax highlighting (highlight.js or Prism)
- Copy-to-clipboard for code blocks
- Message regeneration (retry button)
- Stop generation button
- Message edit and resend
- Token/cost tracking display
- Error messages with actionable guidance
- Conversation history management
- Streaming progress indicators
- Message timestamps
- Agent status indicators

**Should-Have**:
- Conversation export (JSON, Markdown)
- Conversation import
- Agent avatar/icon customization
- System message display with different styling
- Conversation tagging/categorization
- Search within conversation
- Agent response rating (thumbs up/down)
- Keyboard shortcuts (Ctrl+Enter, Ctrl+K, etc.)

### Medium Priority (Phase 3-4)

**Nice-to-Have**:
- Conversation branching (explore alternatives)
- Diff view for code changes
- Message bookmarking
- Conversation templates/starters
- Agent suggestions/autocomplete
- Message formatting toolbar
- Conversation analytics dashboard
- Agent performance metrics
- Responsive mobile layout
- Drag-and-drop file attachment (Phase 2+)

### Future Considerations (Phase 5+)

**Long-Term**:
- Multi-language support (i18n)
- Agent-to-agent communication visualization
- Voice input (speech-to-text)
- Voice output (text-to-speech)
- Conversation sharing (link, embed)
- Agent marketplace/registry
- Conversation replay with different parameters
- Collaborative editing (multi-user)
- Agent fine-tuning integration
- Real-time collaboration features

## Known Challenges

### High Effort Challenges

1. **Blazor Server State Management**
   - Managing conversation state across reconnections
   - Memory management for long-running sessions
   - Distributed scenarios with load balancing
   - Circuit handler integration

2. **Offline State Synchronization** (WASM)
   - Conflict resolution when reconnecting
   - Merging server state with local changes
   - Detecting and handling stale data

3. **WASM Bundle Size with MudBlazor**
   - MudBlazor adds significant size (~500KB+)
   - Balancing features vs bundle size
   - Lazy loading strategies for advanced features

4. **API Surface Design for Extensibility**
   - Balancing simplicity vs extensibility
   - Avoiding breaking changes across versions
   - Clear distinction between public and internal APIs

### Medium Effort Challenges

1. **SignalR Connection Lifecycle** (Server)
   - Handling reconnections during streaming
   - Detecting stale connections
   - Graceful degradation

2. **Concurrent Streaming Sessions** (Server)
   - Multiple users on same server instance
   - Resource contention and throttling
   - Memory pressure

3. **Network Reliability Detection** (WASM)
   - False positives/negatives in detection
   - Handling slow networks vs offline
   - Mobile network switching scenarios

4. **Browser Storage Limits** (WASM)
   - LocalStorage 5-10MB limits
   - IndexedDB for large conversations
   - Quota management and cleanup

5. **Testing Both Modes** (Hybrid)
   - Ensuring components work in both Server and WASM
   - Mode-specific integration tests
   - CI/CD for both modes

## Success Criteria

### Technical Success

**Performance**:
- Blazor Server: < 1ms first token latency achieved
- Blazor WASM: < 50ms first token latency, < 2s reconnection
- Bundle size: < 50KB additional overhead (base package)
- Time to working chat: < 5 minutes from `dotnet new`

**Quality**:
- All samples run out-of-the-box
- Error messages are helpful and actionable
- Components support MudBlazor theming
- Works with .NET 8.0 and 9.0

### Adoption Success

**Metrics** (3 months / 6 months):
- NuGet downloads: 1,000 / 5,000
- GitHub stars: 50 / 200
- Sample app forks: 20 / 100
- Blog mentions: 3 / 10
- AG-UI community: Mentioned / Official link

### User Experience Success

- Developers can add AI chat to Blazor apps in < 10 lines of code
- MudBlazor components are visually stunning
- Offline support works reliably in WASM
- Documentation is comprehensive and clear
- Test utilities make testing easy

## Out of Scope

### Explicitly NOT Included

- **Custom AG-UI protocol implementation** - Use Microsoft's implementation
- **Custom `IAgentBackend` abstraction** - Use `IChatClient`
- **Custom event types** - Use Microsoft's AG-UI events
- **Custom message types** - Use `ChatMessage`
- **Custom tool calling** - Use `FunctionCallContent`
- **SSE transport implementation** - Use Microsoft's `MapAGUI()`
- **Non-MudBlazor UI frameworks in Phase 1** - Radzen, Syncfusion deferred
- **Multi-user collaboration in Phase 1** - Single user per conversation
- **Real-time collaboration in Phase 1** - Deferred to Phase 3+
- **File handling components in Phase 1** - Document `InputFile` integration, add components in Phase 2+
- **Agent marketplace/registry** - Very long-term consideration
- **Agent fine-tuning UI** - Out of scope, focus on consumption

## Implementation Phases (High-Level)

### Phase 0: Foundation (Week 1-2)
- Project structure and build system
- Base abstractions and interfaces
- Minimal base package skeleton
- CI/CD pipeline setup

### Phase 1: MudBlazor MVP (Week 3-5)
- Core MudBlazor components (Chat, MessageList, Input)
- Blazor Server direct streaming
- Basic WASM HTTP client
- BlazorServer.Basic sample
- Essential message rendering (Markdown, syntax highlighting)

### Phase 2: Production Hardening (Week 6-8)
- Conversation history management
- State persistence (LocalStorage, Redis)
- Offline support for WASM
- Error handling and retry logic
- Tool approval UI
- Token/cost tracking
- BlazorWasm.Basic sample

### Phase 3: Advanced Features (Week 9-11)
- Advanced MudBlazor components (EventLog, StateViewer)
- Message operations (edit, regenerate, stop)
- Conversation management (export, import, search)
- Full samples (Server.Full, Wasm.Full)
- Mobile-responsive layout
- Keyboard shortcuts

### Phase 4: Polish & Extensibility (Week 12-13)
- Testing package with mocks
- Performance optimizations (virtualization)
- Extension points documentation
- Component customization hooks
- Accessibility improvements
- Hybrid sample

### Phase 5: Documentation & Launch (Week 14-15)
- Comprehensive documentation
- Video tutorials
- Blog post
- NuGet package publication
- Community announcement

### Phase 6: Post-Launch (Ongoing)
- Community feedback incorporation
- Bug fixes and performance tuning
- Additional samples based on demand
- Consideration of Phase 2+ features

## Relationship to Axi

- **Axi consumes this library** as a NuGet dependency
- **Axi-specific features** (state observers, sandboxing) stay in Axi
- This library is **generic Blazor + AG-UI**, not Axi-specific
- Axi can extend components via inheritance or composition
- This library provides foundation, Axi adds specialized features

## References

### Documentation
- Original PRP: [plan/10-specs/00-PRP/PRP.md](./PRP.md)
- Intent Analysis: [plan/10-specs/00-PRP/intent.md](./intent.md)
- Target Audience Assessment: [plan/20-scoping/target-audiences-assessment.md](../../20-scoping/target-audiences-assessment.md)
- Inferred Features: [plan/20-scoping/inferred-features.md](../../20-scoping/inferred-features.md)
- Clarified Answers: [plan/10-specs/00-PRP/15-clarifying-questions/clarified-answers.md](./15-clarifying-questions/clarified-answers.md)

### External Resources
- [Microsoft Agent Framework AG-UI Integration](https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/)
- [Getting Started with AG-UI](https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/getting-started)
- [AG-UI Protocol Docs](https://docs.ag-ui.com/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [MudBlazor Documentation](https://mudblazor.com/)

---

**This clarified PRP incorporates decisions from 10 clarifying questions and analysis of 6 target audiences, identifying 45 inferred features organized by priority. The project is ready for phase planning and epic creation.**
