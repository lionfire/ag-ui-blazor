# Clarified Answers

This document contains the final answers to all clarifying questions from Round 1.

## UI Framework Dependencies

### Q: Should this library have a hard dependency on a UI framework like MudBlazor, or should it be framework-agnostic?

**A**: MudBlazor is the primary focus with rich, full-featured support:
- `LionFire.AgUi.Blazor` - Minimal base package (basic HTML/CSS only if needed at all)
- `LionFire.AgUi.Blazor.MudBlazor` - Rich, full-featured package with MudBlazor components (PRIMARY FOCUS)
- Other UI frameworks (Radzen, Syncfusion) could come later but are NOT a priority now

**Implications**:
- MudBlazor components will be beautiful, feature-rich, and production-ready from day one
- Base package exists primarily for abstractions and shared logic
- MudBlazor package will have dependency on MudBlazor NuGet packages
- Larger bundle size is acceptable in exchange for rich UI components

---

## State Persistence Strategy

### Q: How should conversation state be persisted?

**A**: Provide optional state persistence via `IAgentStateManager` interface:
- Blazor Server: In-memory by default, optional distributed cache (Redis, SQL)
- Blazor WASM: LocalStorage by default, optional IndexedDB for large conversations
- Developers can implement custom persistence by implementing the interface

**Implications**:
- Flexible architecture allows different storage backends
- Default behavior is sensible (in-memory for Server, LocalStorage for WASM)
- Enterprise scenarios can use Redis/SQL for server-side persistence
- WASM apps can work offline with LocalStorage

---

## Authentication Integration

### Q: How should the library handle authentication for AG-UI endpoints?

**A**: Authentication-agnostic at the component level, but provide samples and documentation showing:
- JWT bearer tokens for WASM clients
- Cookie authentication for Server
- Azure AD / OAuth integration examples
- Custom authentication headers via HttpClient configuration

For WASM, provide `AuthenticationMessageHandler` support in the HttpClient setup.

**Implications**:
- Components don't enforce specific auth patterns
- Samples demonstrate common scenarios (JWT, cookies, Azure AD)
- HttpClient configuration allows custom auth headers
- Works with ASP.NET Core Identity, Azure AD, or custom solutions

---

## File Upload/Download Support

### Q: Should we provide Blazor components for file upload/download?

**A**: Phase 1 - No built-in file components, document how to use Blazor's `InputFile` with agents. Phase 2+ - Add optional `FileAttachment` component that integrates with the chat UI:
- Drag-and-drop file upload
- File preview thumbnails
- Progress indication
- Download links for agent-generated files

**Implications**:
- Phase 1 focuses on core chat functionality
- Documentation shows integration with standard Blazor InputFile
- Phase 2+ adds richer file handling for better UX
- File components are optional, not required

---

## Tool Approval User Experience

### Q: For human-in-the-loop tool approval, should the component block the UI or use async notifications?

**A**: Provide both patterns:
- **Default (blocking)**: Modal dialog blocks until approval/deny
- **Optional (async)**: Toast notifications, approval queue UI, agent continues other work

Let developers choose via component parameter: `ToolApprovalMode="Blocking|Async"`

**Implications**:
- Simple scenarios use blocking modal (default)
- Complex scenarios use async mode for better UX
- MudBlazor's `MudDialog` and `MudSnackbar` perfect for this
- Developer controls behavior via parameter

---

## Multi-Agent Conversation Mixing

### Q: Should the library support multi-agent conversations?

**A**: Phase 1 - Single agent per conversation with `AgentSelector` to switch between agents (new thread per agent). Phase 2+ - Support for multi-agent threads where multiple agents can participate in the same conversation:
- Agent handoff patterns
- Agent collaboration UI
- Clear visual distinction between agents (avatars, colors)
- Conversation routing logic

**Implications**:
- Phase 1 focuses on single-agent simplicity
- AgentSelector component allows switching between agents
- Phase 2+ enables advanced multi-agent scenarios
- Architecture allows for future expansion

---

## Real-time Collaboration

### Q: Should multiple users be able to participate in the same conversation simultaneously?

**A**: Phase 1 - Single user per conversation. Document how to implement multi-user using SignalR groups. Phase 3+ - Optional multi-user support:
- Real-time cursor/typing indicators
- User presence indicators
- Conflict resolution for simultaneous inputs
- Require explicit opt-in via configuration

**Implications**:
- Phase 1 keeps architecture simple
- Documentation provides guidance for custom multi-user
- Phase 3+ adds built-in multi-user for enterprise scenarios
- Explicit opt-in prevents unexpected behavior

---

## Error Recovery and Retry

### Q: How aggressive should automatic retry and error recovery be?

**A**: Configurable retry policy with sensible defaults:
- Transient errors (network): Auto-retry with exponential backoff (max 3 attempts)
- Rate limits: Auto-retry after rate limit reset time
- Authentication errors: No retry, show error to user
- Server errors (500): One retry, then show error
- Provide `RetryPolicy` configuration option for customization

**Implications**:
- Good defaults provide reliable behavior
- Exponential backoff prevents hammering failed services
- Authentication errors surface immediately (security)
- Developers can customize retry behavior per scenario

---

## Streaming Performance vs Memory

### Q: Should we buffer entire responses or implement virtualization for long messages?

**A**: Phase 1 - Full buffering with warning if message exceeds 100KB. Phase 2+ - Optional virtualization:
- Virtual scrolling for message lists (render only visible items)
- Configurable max message size before virtualization kicks in
- Option to truncate very old messages in long conversations
- Developer-controlled via `EnableVirtualization` and `MaxMessageSize` parameters

**Implications**:
- Phase 1 keeps rendering simple
- MudBlazor's `MudVirtualize` can be used in Phase 2+
- Warnings prevent surprise memory issues
- Developers control performance vs simplicity tradeoff

---

## Testing and Mocking

### Q: Should the library provide test utilities for developers?

**A**: Provide `LionFire.AgUi.Blazor.Testing` package with:
- `FakeAgentClientFactory` that returns mock agents
- `MockAIAgent` with predictable responses
- Test helpers for component testing
- Example test projects showing bUnit integration

**Implications**:
- Separate package keeps runtime lightweight
- Developers can test without real AI APIs
- bUnit integration examples for component testing
- Mock agents provide predictable test behavior
