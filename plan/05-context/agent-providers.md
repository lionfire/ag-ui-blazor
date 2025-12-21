

The following is a chat response for alternatives to 'opencode serve'.


---
### Understanding Your Query
It looks like you're working with OpenCode (the open-source AI coding agent from sst/opencode or opencode-ai/opencode on GitHub), where `opencode serve` spins up an HTTP server for its harness—essentially exposing the agent's capabilities (like code editing, analysis, and tool execution) via an API. This allows remote or programmatic control over the agent, often using protocols like Model Context Protocol (MCP) for tool integration.

You mentioned trying "goose server," but as your CLI output shows, Goose (an AI agent CLI tool) doesn't have a `server` subcommand. However, it *does* support agentic serving via `goose acp` (which runs Goose as an ACP—likely Agent Context Protocol—server on stdio for tool/LLM integration) and `goose mcp` (for running bundled MCP servers, which align closely with OpenCode's tool ecosystem). You could experiment with `goose acp` for stdio-based API-like access, though it's not a full HTTP endpoint out of the box.

If you're looking for **full HTTP API alternatives** (similar to OpenCode's client/server setup with Hono.js backend), here are some strong open-source options based on current tools in the AI coding agent space. These focus on harnesses that provide HTTP endpoints for agent control, code tasks, and integrations. I've prioritized CLI/terminal-based ones that support local models, multiple providers, and agentic workflows (e.g., planning, editing, debugging).

### Recommended Alternatives
I'll list them with key features, setup basics, and how they compare to `opencode serve`. All are open-source and support HTTP APIs for remote driving.

| Tool | Description | HTTP API Details | Setup & Usage | Pros vs. OpenCode | Cons vs. OpenCode |
|------|-------------|------------------|---------------|-------------------|-------------------|
| **AgentAPI** (coder/agentapi) | A lightweight HTTP wrapper that proxies control over multiple coding agents (including Goose, Aider, Codex CLI, and even OpenCode itself). It's essentially a unified API server for agent harnesses. | Runs a simple HTTP server (e.g., on localhost:8080) with endpoints for chat, sessions, and tool calls. Supports JSON payloads for prompts, file ops, and multi-turn interactions. | Download binary: `curl -fsSL https://github.com/coder/agentapi/releases/latest/download/agentapi-$(uname -s | tr '[:upper:]' '[:lower:]')-$(uname -m | sed 's/x86_64/amd64/;s/aarch64/arm64/') -o agentapi && chmod +x agentapi`<br>Run: `agentapi server --goose --model claude-3.5-sonnet`<br>API example: `curl -X POST http://localhost:8080/chat -d '{"prompt": "Refactor this function..."}'` | Broader agent support (e.g., integrate Goose's ACP directly); minimal overhead; easy to chain agents. | Less "native" to one harness—it's a meta-tool; requires specifying agent type (e.g., `--type=goose`). |
| **Aider** | Terminal-based AI pair programmer with strong agentic features (edits code in-place, handles git, supports multi-file tasks). | Built-in HTTP server mode via API flags; exposes endpoints for sessions, edits, and repo interactions. Integrates with MCP-like tools. | Install: `pip install aider-chat`<br>Run server: `aider --server --model openai/gpt-4o`<br>Use API: Send POST requests to `/edit` or `/chat` with JSON (e.g., include code diffs). | Excellent for git-aware workflows; supports local models (Ollama/LM Studio); more battle-tested for real coding tasks. | API is more edit-focused than full TUI; stdio-heavy by default (use `--http` for full API). |
| **Cline** (or RooCode/Cline variants) | VS Code extension with CLI fallback for agentic coding (multi-file analysis, checkpoints, debugging). Often paired with local servers like LM Studio. | Exposes an HTTP API via its backend server for orchestration (e.g., tool calls, session management). Supports MCP for external tools. | Install via VS Code marketplace or CLI: `npm install -g cline`<br>Run: `cline server --model local/ollama --port 8080`<br>API: Endpoints like `/task` for agent runs (JSON input with goals/files). | Great local-first setup (e.g., with Devstral models); checkpointing prevents bad edits; VS Code integration if you need GUI. | More IDE-tied (though CLI works); setup can involve custom prompts for tool calling. |
| **Void** | Open-source terminal agent similar to OpenCode, focused on privacy and local execution. | Full HTTP server for remote control, with endpoints for prompts, file I/O, and agent modes (build/plan). | Install: `cargo install void-agent` (Rust-based)<br>Run: `void serve --model claude --api-port 3000`<br>API: RESTful calls to `/session` or `/execute`. | Fully local/privacy-focused; lightweight and extensible with custom tools. | Newer project—fewer integrations; Rust dependency might add build time. |
| **Droid** (by Factory) | High-performing CLI agent for debugging/automation; tops benchmarks like Terminal-Bench. | HTTP API server for task orchestration (e.g., multi-step code fixes). Supports OpenRouter/local models. | Install: `pip install droid-factory`<br>Run: `droid server --model gemini-1.5-pro`<br>API: POST to `/run` with task JSON. | Superior at complex debugging; fast inference with free tiers (e.g., Cerebras backend). | Less emphasis on TUI—more scriptable; API docs are evolving. |

### Quick Tips
- **If sticking close to Goose**: Use `goose acp` for stdio-based serving (pipe inputs via tools like `nc` for pseudo-API), or wrap it with AgentAPI for true HTTP. Goose's `mcp` is great for tool discovery, similar to OpenCode's MCP support.
- **Local Models**: All these pair well with LM Studio or Ollama for offline HTTP endpoints (e.g., `http://localhost:1234/v1/chat/completions`).
- **Testing**: Start with AgentAPI—it's the quickest way to get an HTTP layer on Goose without much config.
- **More Exploration**: Check AGENTS.md format (used by 60k+ projects) for standardizing prompts across these tools.

If none hit the mark or you share more details (e.g., specific models or workflows), I can refine suggestions!
