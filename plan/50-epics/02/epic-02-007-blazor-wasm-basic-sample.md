---
greenlit: true
status: done
completed: 2025-12-11
---

# Epic 02-007: BlazorWasm.Basic Sample

**Phase**: 02 - Production Hardening
**Estimated Effort**: 3-4 days
**Status**: COMPLETE

## Overview
Create Blazor WASM sample with server and client projects demonstrating offline support and reconnection.

## Implementation Tasks
- [x] Create `samples/BlazorWasm.Basic/` directory structure
- [x] Create Server project with AG-UI endpoint
- [x] Create Client project with WASM app
- [x] Create Shared project for models
- [x] Configure Server Program.cs:
  - [x] Add AGUI services
  - [x] Map AGUI endpoint
  - [x] Register mock IChatClient
- [x] Configure Client Program.cs:
  - [x] Add AgUiBlazorWasm services
  - [x] Configure server URL
  - [x] Enable offline mode
- [x] Update Client App.razor to use MudAgentChat
- [x] Add MudThemeProvider for theme support
- [x] Test offline behavior (network tab)
- [x] Add README with setup instructions
- [x] Document offline testing
- [x] Time setup and verify < 10 minutes

## Acceptance Criteria
- [x] Server and client run together
- [x] Chat works when online
- [x] Messages queue when offline
- [x] Auto-reconnect works
- [x] Sample runs without configuration
- [x] README is clear
- [x] Setup < 10 minutes

## Implementation Summary

### Files Created:

**Server Project:**
- `samples/BlazorWasm.Basic/Server/BlazorWasm.Basic.Server.csproj` - Server project file
- `samples/BlazorWasm.Basic/Server/Program.cs` - API endpoints for AG-UI protocol
- `samples/BlazorWasm.Basic/Server/MockChatClient.cs` - Demo chat client

**Client Project:**
- `samples/BlazorWasm.Basic/Client/BlazorWasm.Basic.Client.csproj` - Client project file
- `samples/BlazorWasm.Basic/Client/Program.cs` - DI configuration
- `samples/BlazorWasm.Basic/Client/App.razor` - Blazor app root
- `samples/BlazorWasm.Basic/Client/_Imports.razor` - Global usings
- `samples/BlazorWasm.Basic/Client/Layout/MainLayout.razor` - App shell with theme
- `samples/BlazorWasm.Basic/Client/Layout/ConnectionStatusIndicator.razor` - Online/offline indicator
- `samples/BlazorWasm.Basic/Client/Pages/Index.razor` - Main chat page
- `samples/BlazorWasm.Basic/Client/wwwroot/index.html` - HTML entry point

**Shared Project:**
- `samples/BlazorWasm.Basic/Shared/BlazorWasm.Basic.Shared.csproj` - Shared project file
- `samples/BlazorWasm.Basic/Shared/AgentInfo.cs` - Common models

**Documentation:**
- `samples/BlazorWasm.Basic/README.md` - Setup and usage instructions

### Key Features:
- Complete AG-UI HTTP API (list agents, chat, streaming)
- MudBlazor UI with dark mode support
- Connection status indicator
- Offline message queuing
- Auto-reconnection
- Zero configuration required to run
