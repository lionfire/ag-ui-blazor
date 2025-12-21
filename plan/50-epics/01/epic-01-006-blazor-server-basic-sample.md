---
greenlit: true
---

# Epic 01-006: BlazorServer.Basic Sample

**Phase**: 01 - MudBlazor MVP
**Status**: Complete
**Estimated Effort**: 2-3 days

## Overview

Create minimal Blazor Server sample that demonstrates < 5 minute setup from `dotnet new` to working AI chat.

**Link to Phase**: [Phase 01: MudBlazor MVP](../../40-phases/01-mudblazor-mvp.md)

## Implementation Tasks

### Project Creation
- [x] Create `samples/BlazorServer.Basic/` directory
- [x] Run `dotnet new blazor -o samples/BlazorServer.Basic -int Server`
- [x] Add to solution
- [x] Add project references to MudBlazor and Server packages

### Program.cs Configuration
- [x] Add MudBlazor services
- [x] Add AgUiBlazorServer services
- [x] Register mock IChatClient for demo
- [x] Register agent using AddAgent()
- [x] Keep code < 20 lines total

### Home Page
- [x] Update Home.razor to use MudAgentChat
- [x] Remove default content
- [x] Add `@using LionFire.AgUi.Blazor.MudBlazor.Components`
- [x] Add `<MudAgentChat AgentName="assistant" />`
- [x] Style for full-page chat

### Mock Agent
- [x] Create simple mock IChatClient
- [x] Returns hardcoded responses for demo
- [x] Supports streaming (yield return characters)
- [x] No external dependencies (no API keys)

### Documentation
- [x] Add README.md with:
  - [x] Prerequisites (.NET 8/9)
  - [x] How to run
  - [x] Expected behavior
  - [x] How to customize

### Verification
- [x] Time setup from `dotnet new` to running chat
- [x] Verify < 5 minutes
- [x] Test with fresh environment
- [x] Verify no configuration needed
- [x] Check error handling works

## Acceptance Criteria

- [x] Sample runs with `dotnet run`
- [x] No configuration or API keys needed
- [x] Chat UI displays and works
- [x] Mock agent responds
- [x] Streaming is visible
- [x] Setup takes < 5 minutes (timed)
- [x] README is clear and complete
- [x] MudBlazor theme switcher works
