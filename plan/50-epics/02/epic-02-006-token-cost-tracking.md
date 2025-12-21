---
greenlit: true
status: done
completed: 2025-12-11
---

# Epic 02-006: Token and Cost Tracking

**Phase**: 02 - Production Hardening
**Estimated Effort**: 2-3 days
**Status**: COMPLETE

## Overview
Add token usage and cost tracking display for messages and conversations.

## Implementation Tasks
- [x] Extract token usage from ChatResponse
- [x] Create TokenUsage model
- [x] Store token usage per message
- [x] Calculate estimated cost (configurable rates)
- [x] Add token display to message UI
- [x] Show tokens per message (small text, optional)
- [x] Add conversation total tokens display
- [x] Create MudTokenSummary component
- [x] Show prompt/completion/total tokens
- [x] Show estimated cost (if configured)
- [x] Format numbers with thousands separator
- [x] Add configuration for cost per token
- [x] Support different models (different costs)
- [x] Unit tests for calculations
- [x] Integration tests for tracking

## Acceptance Criteria
- [x] Token usage displayed accurately
- [x] Cost estimates calculated correctly
- [x] Display is unobtrusive (not cluttering UI)
- [x] Configuration is simple
- [x] Tests pass > 80% coverage

## Implementation Summary

### Files Created/Modified:
- `src/LionFire.AgUi.Blazor/Models/TokenUsage.cs` - Token usage record (already existed)
- `src/LionFire.AgUi.Blazor/Services/TokenCostCalculator.cs` - Cost calculation service with configurable rates
- `src/LionFire.AgUi.Blazor/Extensions/ServiceCollectionExtensions.cs` - DI registration extensions
- `src/LionFire.AgUi.Blazor.MudBlazor/Components/MudTokenSummary.razor` - Token display component (Compact/Inline/Detailed variants)
- `src/LionFire.AgUi.Blazor.MudBlazor/Components/MudTokenSummary.razor.cs` - Component code-behind
- `tests/LionFire.AgUi.Blazor.Tests/Services/TokenCostCalculatorTests.cs` - 18 unit tests

### Key Features:
- TokenCostCalculator service with configurable per-model pricing
- Default rates for GPT-4, GPT-4o, GPT-3.5-turbo, Claude-3-opus/sonnet/haiku
- Partial model name matching for variants (e.g., gpt-4-0314 matches gpt-4)
- MudTokenSummary component with three display variants:
  - Compact: Small chip with tooltip
  - Inline: Text display with token breakdown
  - Detailed: Full card with all statistics
- Configurable currency symbol
- FormatCost with adaptive precision for small amounts
- FormatTokenCount with thousands separator
