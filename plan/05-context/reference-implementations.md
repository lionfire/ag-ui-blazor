# Reference Implementations & Design Resources

This document catalogs valuable resources from the AG-UI and CopilotKit repositories to consult during Blazor component implementation.

## AG-UI Repository (`/dv/ag-ui`)

### Protocol Documentation (Must-Read)

| Document | Path | Key Content |
|----------|------|-------------|
| Architecture | `/dv/ag-ui/docs/concepts/architecture.mdx` | Event-driven design, transport-agnostic patterns |
| Events | `/dv/ag-ui/docs/concepts/events.mdx` | All 16+ event types with examples |
| Messages | `/dv/ag-ui/docs/concepts/messages.mdx` | Message types, streaming patterns |
| Tools | `/dv/ag-ui/docs/concepts/tools.mdx` | Tool system, approval workflows |
| State | `/dv/ag-ui/docs/concepts/state.mdx` | Snapshots, deltas, JSON Patch (RFC 6902) |
| Middleware | `/dv/ag-ui/docs/concepts/middleware.mdx` | Event transformation and filtering |

### Event System (Critical for UI)

**Event Categories:**
- **Lifecycle**: `RUN_STARTED`, `RUN_FINISHED`, `RUN_ERROR`, `STEP_STARTED`, `STEP_FINISHED`
- **Text Streaming**: `TEXT_MESSAGE_START` → `TEXT_MESSAGE_CONTENT` → `TEXT_MESSAGE_END`
- **Thinking/Reasoning**: `THINKING_TEXT_MESSAGE_START/END`, `THINKING_START/END`
- **Tool Calls**: `TOOL_CALL_START` → `TOOL_CALL_ARGS` → `TOOL_CALL_END` → `TOOL_CALL_RESULT`
- **State Management**: `STATE_SNAPSHOT`, `STATE_DELTA` (JSON Patch)

**Source Types:**
- `/dv/ag-ui/sdks/typescript/packages/core/src/events.ts`
- `/dv/ag-ui/sdks/typescript/packages/core/src/types.ts`

### Message Roles

```
user | assistant | system | tool | developer | activity
```

**Activity Messages** are frontend-only and updateable - useful for progress indicators.

### State Synchronization Pattern

1. `STATE_SNAPSHOT` - Complete state at initialization
2. `STATE_DELTA` - Incremental updates via JSON Patch (RFC 6902)
3. Operations: `add`, `remove`, `replace`, `move`, `copy`, `test`

---

## AG-UI Dojo (`/dv/ag-ui/apps/dojo/`)

Reference UI implementation showcasing AG-UI patterns.

### UI Components to Study

| Component | Path | Key Patterns |
|-----------|------|--------------|
| Button | `/dv/ag-ui/apps/dojo/src/components/ui/button.tsx` | CVA variants, a11y focus states |
| Theme Toggle | `/dv/ag-ui/apps/dojo/src/components/theme-toggle.tsx` | Light/dark switching |
| Theme Provider | `/dv/ag-ui/apps/dojo/src/components/theme-provider.tsx` | next-themes pattern |

### Feature Examples

| Feature | Path | Lines | Purpose |
|---------|------|-------|---------|
| Simple Chat | `.../feature/agentic_chat/page.tsx` | 82 | Minimal chat implementation |
| Human-in-the-Loop | `.../feature/human_in_the_loop/page.tsx` | 510 | **Best reference** - complete approval UI |
| Shared State | `.../feature/shared_state/` | - | Bidirectional state sync |
| Tool-Based Gen UI | `.../feature/tool_based_generative_ui/` | - | Dynamic UI generation |
| Reasoning Display | `.../feature/agentic_chat_reasoning/` | - | Chain-of-thought visibility |

### Human-in-the-Loop UI Patterns (from Dojo)

**Components demonstrated:**
- `StepContainer` - Card-based step selection with gradient backgrounds
- `StepHeader` - Progress indicator with enabled/total count
- `StepItem` - Checkbox-based selection with hover/enabled states
- `ActionButton` - Multi-variant button (primary, secondary, success, danger)

**UX Patterns:**
- Visual feedback on selection (gradients, borders)
- Progress bars showing completion percentage
- Disabled states during execution
- Success/failure result display with theme-aware colors
- Accept/Reject button patterns

### Theming Approach

**CSS Custom Properties:**
```css
--copilot-kit-primary: #6963ff;
--copilot-kit-background-color: ...;
```

**Tailwind Dark Mode:**
- Uses `class` strategy (not `media`)
- `dark:` prefix for dark variants
- Opacity utilities: `bg-blue-900/30`

**For MudBlazor:** Map these to MudBlazor's theme system.

### Accessibility Patterns

```css
/* Focus states */
outline-none
focus-visible:border-ring
focus-visible:ring-ring/50
focus-visible:ring-[3px]

/* Error states */
aria-invalid:ring-destructive/20

/* Icon handling */
[&_svg]:pointer-events-none
```

**A11y Best Practices:**
- `sr-only` class for screen readers
- ARIA attributes: `aria-invalid`, `aria-busy`
- Visible focus ring (not outline)
- Keyboard navigation support
- Color contrast meeting WCAG

---

## Microsoft Agent Framework Integration

**Location:** `/dv/ag-ui/integrations/microsoft-agent-framework/`

Shows integration with:
- `IChatClient` abstraction
- `ChatMessage` types
- Tool calling with `FunctionCallContent`
- AG-UI event streaming

**Relevant for:** Understanding how AG-UI maps to Microsoft.Extensions.AI.

---

## Draft Features (Future Consideration)

| Feature | Path | Description |
|---------|------|-------------|
| Interrupts | `/dv/ag-ui/docs/drafts/interrupts.mdx` | Human approval, input gathering |
| Generative UI | `/dv/ag-ui/docs/drafts/generative-ui.mdx` | Schema-driven dynamic UI |
| Reasoning | `/dv/ag-ui/docs/drafts/reasoning.mdx` | Thinking step visibility |

---

## Key Design Principles to Adopt

### From AG-UI Protocol

1. **Event-driven**: All communication through typed events
2. **Transport-agnostic**: Support SSE, WebSockets, or direct streaming
3. **Streaming-first**: Three-event pattern (Start → Content → End)
4. **Frontend-defined tools**: UI controls what tools are available
5. **State via patches**: Efficient updates with JSON Patch

### For Blazor Implementation

1. **Map events to component state**: Each AG-UI event type → Blazor state update
2. **Use SignalR for Blazor Server**: Native streaming, no HTTP overhead
3. **Progressive rendering**: Display content as events arrive
4. **Composable components**: Small, focused, reusable (50-200 lines each)
5. **MudBlazor theming**: Map AG-UI theme concepts to MudTheme

---

## Consultation Checklist

When implementing specific features, consult:

- [ ] **Chat UI**: Dojo `agentic_chat/page.tsx` + Messages doc
- [ ] **Tool Approval**: Dojo `human_in_the_loop/page.tsx` (510 lines - comprehensive)
- [ ] **Streaming**: Events doc + TypeScript subscriber pattern
- [ ] **State Management**: State doc + JSON Patch RFC 6902
- [ ] **Theming**: Dojo theme components + Tailwind config
- [ ] **Error Handling**: Events doc (RUN_ERROR) + Dojo error display patterns
- [ ] **Accessibility**: Dojo button.tsx focus patterns + ARIA usage
- [ ] **Microsoft Integration**: `/dv/ag-ui/integrations/microsoft-agent-framework/`

---

## CopilotKit Repository (`/dv/CopilotKit`)

CopilotKit is the production React implementation from which AG-UI protocol was extracted. Contains battle-tested UI patterns.

### Component Architecture

**Location:** `/dv/CopilotKit/CopilotKit/packages/react-ui/src/components/`

```
react-ui/src/
├── components/
│   ├── chat/
│   │   ├── Chat.tsx              # Main orchestrator
│   │   ├── ChatContext.tsx       # Theme/label context
│   │   ├── Messages.tsx          # Message list + scroll
│   │   ├── Input.tsx             # User input textarea
│   │   ├── messages/
│   │   │   ├── AssistantMessage.tsx
│   │   │   ├── UserMessage.tsx
│   │   │   ├── RenderMessage.tsx # Unified renderer
│   │   │   └── ImageRenderer.tsx
│   │   ├── Modal.tsx, Popup.tsx, Window.tsx
│   │   └── Markdown.tsx          # Content rendering
│   └── [other features]
└── css/                          # CSS variable styling
```

**Key Patterns:**
- Component composition via props (all sub-components replaceable)
- Context API for theme/icon management
- Ref forwarding for imperative control

### Chat UI Components

| Component | File | Key Features |
|-----------|------|--------------|
| Chat | `Chat.tsx` | Master orchestrator, layout variants |
| Messages | `Messages.tsx` | Auto-scroll, scroll detection |
| Input | `Input.tsx` | Auto-resize textarea, send/stop |
| AssistantMessage | `messages/AssistantMessage.tsx` | Copy, regenerate, feedback buttons |
| UserMessage | `messages/UserMessage.tsx` | User message display |
| Markdown | `Markdown.tsx` | Streaming indicator, syntax highlighting |

### Streaming Text Display

**Streaming Indicator Pattern:**
```tsx
// Shows pulsing "▍" cursor during streaming
// Character with pulse animation for real-time token display
```

**Message Features:**
- Real-time streaming indicator (pulsing cursor)
- Copy-to-clipboard functionality
- Regenerate/retry buttons
- Thumbs up/down feedback
- Markdown with code syntax highlighting

### Auto-Scroll Implementation

```typescript
// Messages.tsx pattern:
// - useScrollToBottom() hook tracks scroll state
// - Detects user scroll-up to prevent jumping
// - Auto-scrolls only when viewing latest messages
```

### Tool Approval / Human-in-the-Loop

**Core Pattern: `renderAndWaitForResponse`**

```typescript
// Status states: "inProgress" | "executing" | "complete"

useCopilotAction({
  name: "actionName",
  renderAndWaitForResponse: ({ args, respond, status }) => {
    return (
      <ApprovalUI
        onApprove={() => respond({ approved: true })}
        onReject={() => respond({ approved: false })}
      />
    );
  }
});
```

**Status Flow:**
1. `"inProgress"` → Partial args arriving (streaming)
2. `"executing"` → Full args ready, awaiting human response
3. `"complete"` → Response sent, result available

**Example Locations:**
- Step selector: `/examples/ag2/human_in_the_loop/`
- Form collection: `/examples/copilot-fully-custom/Chat.tsx`

### CSS Variable Theme System

**Location:** `/css/colors.css`

```css
:root {
  /* Primary */
  --copilot-kit-primary-color: rgb(28, 28, 28);
  --copilot-kit-contrast-color: rgb(255, 255, 255);

  /* Backgrounds */
  --copilot-kit-background-color: rgb(255 255 255);
  --copilot-kit-input-background-color: #fbfbfb;
  --copilot-kit-secondary-color: rgb(255 255 255);

  /* Text */
  --copilot-kit-secondary-contrast-color: rgb(28, 28, 28);

  /* Borders */
  --copilot-kit-separator-color: rgb(200 200 200);
  --copilot-kit-muted-color: rgb(200 200 200);

  /* Errors */
  --copilot-kit-error-background: #fef2f2;
  --copilot-kit-error-border: #fecaca;
  --copilot-kit-error-text: #dc2626;

  /* Shadows */
  --copilot-kit-shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --copilot-kit-shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --copilot-kit-shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}
```

**Dark Mode:**
```css
.dark, html.dark, [data-theme="dark"] {
  --copilot-kit-primary-color: rgb(255, 255, 255);
  --copilot-kit-background-color: rgb(17, 17, 17);
  /* ... inverted colors */
}
```

### CSS Files by Feature

| File | Purpose |
|------|---------|
| `colors.css` | Theme variables, color system |
| `animations.css` | Spinner, pulse, activity dots |
| `button.css` | Chat toggle button |
| `messages.css` | Message container, controls |
| `input.css` | Input textarea, buttons |
| `header.css` | Header area |
| `panel.css` | Main chat panel |
| `popup.css` | Fixed bottom-right popup |
| `window.css` | Modal window |
| `sidebar.css` | Sidebar layout |

### Accessibility

**ARIA Labels Found:**
- `aria-label="Close Chat"`
- `aria-label="Regenerate Response"`
- `aria-label="Copy to Clipboard"`
- `aria-label="Thumbs Up"` / `"Thumbs Down"`

**Keyboard Support:**
- Enter sends message (Shift+Enter for newline)
- Escape closes modal (`hitEscapeToClose` prop)
- Cmd/Ctrl + `/` keyboard shortcut
- Tab navigation through controls

**Mobile:**
```css
@media (max-width: 768px) {
  /* Always show message controls on mobile */
  .copilotKitMessage .copilotKitMessageControls {
    opacity: 1;
  }
}
```

### Hooks to Understand

| Hook | Purpose | Blazor Equivalent |
|------|---------|-------------------|
| `useCopilotChat()` | Main chat state | Chat service/state |
| `useCopilotAction()` | Action registration | Tool handlers |
| `useCopilotReadable()` | Context sharing | Cascading values |
| `usePushToTalk()` | Voice input | JS interop |
| `useCopyToClipboard()` | Clipboard ops | JS interop |

### Example Applications

| Example | Path | Demonstrates |
|---------|------|--------------|
| Form Filling | `/examples/copilot-form-filling/` | React Hook Form + Zod integration |
| Fully Custom | `/examples/copilot-fully-custom/` | Custom components, generative UI |
| Human-in-the-Loop | `/examples/ag2/human_in_the_loop/` | Step selection, approval workflow |
| Feature Viewer | `/examples/ag2/feature-viewer/` | Multiple feature demos |

### Component Customization API

```tsx
<CopilotChat
  AssistantMessage={CustomAssistant}  // Replace assistant renderer
  UserMessage={CustomUser}            // Replace user renderer
  Input={CustomInput}                 // Replace input component
  icons={customIcons}                 // Replace icons
  labels={customLabels}               // Replace labels
/>
```

---

## Design Principles Summary (Both Repos)

1. **Composition over Inheritance** - All components replaceable via props
2. **CSS Variables First** - Single source of truth for theming
3. **Streaming First** - Built for real-time token generation
4. **Accessibility Built-in** - ARIA labels, keyboard nav, semantic HTML
5. **State Separation** - UI state separate from chat logic
6. **Context for Customization** - Theme/icons via context
7. **Flexible Rendering** - Markdown components replaceable
8. **Error Resilience** - Global and local error handlers

---

## Updated Consultation Checklist

When implementing specific features, consult:

### AG-UI Repository
- [ ] **Protocol Events**: `/dv/ag-ui/docs/concepts/events.mdx`
- [ ] **State Management**: `/dv/ag-ui/docs/concepts/state.mdx`
- [ ] **Tool System**: `/dv/ag-ui/docs/concepts/tools.mdx`
- [ ] **Human-in-the-Loop**: `/dv/ag-ui/apps/dojo/.../human_in_the_loop/page.tsx`
- [ ] **Microsoft Integration**: `/dv/ag-ui/integrations/microsoft-agent-framework/`

### CopilotKit Repository
- [ ] **Chat Component**: `/dv/CopilotKit/.../react-ui/src/components/chat/Chat.tsx`
- [ ] **Message Rendering**: `/dv/CopilotKit/.../chat/messages/AssistantMessage.tsx`
- [ ] **Streaming Display**: `/dv/CopilotKit/.../chat/Markdown.tsx`
- [ ] **Auto-Scroll**: `/dv/CopilotKit/.../chat/Messages.tsx`
- [ ] **Tool Approval**: `/dv/CopilotKit/examples/ag2/human_in_the_loop/`
- [ ] **Theme Variables**: `/dv/CopilotKit/.../react-ui/src/css/colors.css`
- [ ] **Full Customization**: `/dv/CopilotKit/examples/copilot-fully-custom/`
