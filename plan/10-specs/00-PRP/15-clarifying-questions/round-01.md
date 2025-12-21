# Clarifying Questions - Round 1

Note: if you wish to accept the proposed answer, you can leave it as is and the proposed answer will be assumed to be your answer.

## Question 1: UI Framework Dependencies

**Question**: Should this library have a hard dependency on a UI framework like MudBlazor, or should it be framework-agnostic with basic HTML/CSS that developers can style themselves?

**Why this matters**: This affects architecture decisions, bundle size, theming approach, and the flexibility developers have. A framework dependency provides richer components out-of-the-box but limits choices. Framework-agnostic means more styling work for developers but maximum flexibility.

**USER'S ANSWER**: MudBlazor is the primary focus with rich, full-featured support:
- `LionFire.AgUi.Blazor` - Minimal base package (basic HTML/CSS only if needed at all)
- `LionFire.AgUi.Blazor.MudBlazor` - Rich, full-featured package with MudBlazor components (PRIMARY FOCUS)
- Other UI frameworks (Radzen, Syncfusion) could come later but are NOT a priority now

This means we prioritize beautiful, rich MudBlazor components from the start rather than framework-agnostic minimal components.

**USER ACCEPTED** proposed answers for Questions 2-10.

---

## Question 2: State Persistence Strategy

**Question**: How should conversation state be persisted? Should we provide built-in persistence or leave it entirely to the developer?

**Why this matters**: Users expect conversations to survive page refreshes and browser restarts. The approach affects architecture, storage requirements, and developer experience. Different hosting modes (Server vs WASM) may need different strategies.

**Proposed answer**: Provide optional state persistence via `IAgentStateManager` interface:
- Blazor Server: In-memory by default, optional distributed cache (Redis, SQL)
- Blazor WASM: LocalStorage by default, optional IndexedDB for large conversations
- Developers can implement custom persistence by implementing the interface

**Alternatives to consider**:
- Option A: No built-in persistence, developers handle it entirely (simplest for library, most work for developers)
- Option B: Always persist to storage (may be unexpected behavior, privacy concerns)
- Option C: Server-side database persistence only (doesn't work for WASM offline)

---

## Question 3: Authentication Integration

**Question**: How should the library handle authentication for AG-UI endpoints? Should it integrate with ASP.NET Core authentication or be authentication-agnostic?

**Why this matters**: Production applications need to secure AG-UI endpoints. The approach affects how developers configure the library and what security patterns they can use.

**Proposed answer**: Authentication-agnostic at the component level, but provide samples and documentation showing:
- JWT bearer tokens for WASM clients
- Cookie authentication for Server
- Azure AD / OAuth integration examples
- Custom authentication headers via HttpClient configuration

For WASM, provide `AuthenticationMessageHandler` support in the HttpClient setup.

**Alternatives to consider**:
- Option A: Built-in JWT handling (opinionated, less flexible)
- Option B: Require authentication configuration (may be too complex)
- Option C: No authentication support (insecure, not production-ready)

---

## Question 4: File Upload/Download Support

**Question**: AG-UI supports file operations. Should we provide Blazor components for file upload/download, or leave this to developers using standard Blazor InputFile?

**Why this matters**: File handling is mentioned in the PRP as an open question. Users may want to upload documents to agents or download generated files. The approach affects component complexity and feature completeness.

**Proposed answer**: Phase 1 - No built-in file components, document how to use Blazor's `InputFile` with agents. Phase 2+ - Add optional `FileAttachment` component that integrates with the chat UI:
- Drag-and-drop file upload
- File preview thumbnails
- Progress indication
- Download links for agent-generated files

**Alternatives to consider**:
- Option A: Full file handling in Phase 1 (delays MVP)
- Option B: Never provide file components (leaves gap in feature set)
- Option C: Separate NuGet package for file handling (modularity, discovery issue)

---

## Question 5: Tool Approval User Experience

**Question**: For human-in-the-loop tool approval, should the component block the UI waiting for approval, or should it be async with notifications?

**Why this matters**: This affects user experience significantly. Blocking UX is simple but can be frustrating. Async UX is better for long-running agents but more complex to implement.

**Proposed answer**: Provide both patterns:
- **Default (blocking)**: Modal dialog blocks until approval/deny
- **Optional (async)**: Toast notifications, approval queue UI, agent continues other work

Let developers choose via component parameter: `ToolApprovalMode="Blocking|Async"`

**Alternatives to consider**:
- Option A: Blocking only (simpler, may frustrate users with multiple pending approvals)
- Option B: Async only (better UX, more complex, may confuse simple use cases)
- Option C: No approval UI, use events only (most flexible, most work for developers)

---

## Question 6: Multi-Agent Conversation Mixing

**Question**: Should the library support conversations that involve multiple agents (agent handoffs, agent collaboration), or just single-agent conversations?

**Why this matters**: Multi-agent scenarios are increasingly common (specialist agents, agent orchestration). The architecture decisions affect extensibility and component design.

**Proposed answer**: Phase 1 - Single agent per conversation with `AgentSelector` to switch between agents (new thread per agent). Phase 2+ - Support for multi-agent threads where multiple agents can participate in the same conversation:
- Agent handoff patterns
- Agent collaboration UI
- Clear visual distinction between agents (avatars, colors)
- Conversation routing logic

**Alternatives to consider**:
- Option A: Multi-agent from day one (delays MVP, complex)
- Option B: Single agent only (simpler, limits use cases)
- Option C: Leave multi-agent to frameworks like Axi (misses opportunity)

---

## Question 7: Real-time Collaboration

**Question**: Should multiple users be able to view/participate in the same conversation simultaneously (like Google Docs collaboration)?

**Why this matters**: Enterprise scenarios may involve team collaboration with agents (code reviews, brainstorming, pair programming). This affects state synchronization architecture significantly.

**Proposed answer**: Phase 1 - Single user per conversation. Document how to implement multi-user using SignalR groups. Phase 3+ - Optional multi-user support:
- Real-time cursor/typing indicators
- User presence indicators
- Conflict resolution for simultaneous inputs
- Require explicit opt-in via configuration

**Alternatives to consider**:
- Option A: Multi-user from start (very complex, delays MVP significantly)
- Option B: Never support multi-user (limits enterprise use cases)
- Option C: Separate package for multi-user (modularity vs discovery)

---

## Question 8: Error Recovery and Retry

**Question**: How aggressive should automatic retry and error recovery be? Should failed requests auto-retry or require user action?

**Why this matters**: Network issues and API rate limits are common. Too aggressive retry can waste resources and hide issues. Too passive means poor UX.

**Proposed answer**: Configurable retry policy with sensible defaults:
- Transient errors (network): Auto-retry with exponential backoff (max 3 attempts)
- Rate limits: Auto-retry after rate limit reset time
- Authentication errors: No retry, show error to user
- Server errors (500): One retry, then show error
- Provide `RetryPolicy` configuration option for customization

**Alternatives to consider**:
- Option A: No automatic retry (poor UX, but predictable)
- Option B: Always retry indefinitely (waste resources, hide issues)
- Option C: Retry everything aggressively (may mask problems)

---

## Question 9: Streaming Performance vs Memory

**Question**: For very long agent responses, should we buffer the entire response in memory or implement sliding window/virtualization to limit memory usage?

**Why this matters**: AI agents can generate very long responses (thousands of lines of code, long articles). Full buffering is simple but can cause memory issues. Virtualization is better for performance but more complex.

**Proposed answer**: Phase 1 - Full buffering with warning if message exceeds 100KB. Phase 2+ - Optional virtualization:
- Virtual scrolling for message lists (render only visible items)
- Configurable max message size before virtualization kicks in
- Option to truncate very old messages in long conversations
- Developer-controlled via `EnableVirtualization` and `MaxMessageSize` parameters

**Alternatives to consider**:
- Option A: Always virtualize (complexity in Phase 1)
- Option B: Never virtualize (memory issues for long conversations)
- Option C: Hard limits on message size (may truncate important content)

---

## Question 10: Testing and Mocking

**Question**: Should the library provide test utilities, mocks, or fake implementations for testing applications that use these components?

**Why this matters**: Developers need to test their applications without hitting real AI APIs. Providing test utilities improves developer experience and encourages adoption.

**Proposed answer**: Provide `LionFire.AgUi.Blazor.Testing` package with:
- `FakeAgentClientFactory` that returns mock agents
- `MockAIAgent` with predictable responses
- Test helpers for component testing
- Example test projects showing bUnit integration

**Alternatives to consider**:
- Option A: No test utilities (developers figure it out themselves)
- Option B: Built into main package (increases package size for runtime)
- Option C: Only documentation, no code (partial solution)
