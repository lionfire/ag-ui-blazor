# Greenlit Epics - Ready for Implementation

Epics in this file are approved and ready for active development.

## Epics

### Phase 00 - Foundation (COMPLETE)
- ~~00-001~~ (DONE)
- ~~00-002~~ (DONE)
- ~~00-003~~ (DONE)
- ~~00-004~~ (DONE)

### Phase 01 - MudBlazor MVP (COMPLETE)
- ~~01-001~~ (DONE)
- ~~01-002~~ (DONE)
- ~~01-003~~ (DONE)
- ~~01-004~~ (DONE)
- ~~01-005~~ (DONE)
- ~~01-006~~ (DONE)

### Phase 02 - Production Hardening (COMPLETE)
- ~~02-001~~ (DONE)
- ~~02-002~~ (DONE)
- ~~02-003~~ (DONE)
- ~~02-004~~ (DONE)
- ~~02-005~~ (DONE)
- ~~02-006~~ (DONE)
- ~~02-007~~ (DONE)

### Phase 03 - Advanced Features (COMPLETE)
- ~~03-001~~ (DONE)
- ~~03-002~~ (DONE)
- ~~03-003~~ (DONE)
- ~~03-004~~ (DONE)
- ~~03-005~~ (DONE)
- ~~03-006~~ (DONE)
- ~~03-007~~ (DONE)
- ~~03-008~~ (DONE)

## Advisory Notices

## Implementation Order

The epics are listed above in phase and dependency order. Later phases depend on earlier ones.

Recommended implementation sequence:
1. **Phase 00 - Foundation (Required First)**: Complete all 4 epics to establish repository structure, core abstractions, NuGet configuration, and CI/CD pipeline
2. **Phase 01 - MudBlazor MVP**: Build core components (01-001 through 01-006) to create functional chat UI
3. **Phase 02 - Production Hardening**: Add state persistence, WASM support, offline capabilities, and production features
4. **Phase 03 - Advanced Features**: Implement advanced UI features, mobile responsive layout, and comprehensive samples

## Parallel Opportunities

Epics that can be worked on simultaneously:

**Within Phase 00**:
- After repository structure (00-001) completes: 00-002, 00-003 can run in parallel
- 00-004 (CI/CD) requires 00-001, 00-002, 00-003

**Within Phase 01**:
- 01-002 (MudMessageList) and 01-003 (MudMessageInput) can run in parallel
- 01-004 (Markdown Highlighting) and 01-005 (Agent Factory) can run in parallel
- 01-001 (MudAgentChat) requires 01-002, 01-003, 01-005
- 01-006 (Sample) requires 01-001

**Within Phase 02**:
- Most Phase 02 epics can run in parallel after Phase 01 completes
- 02-002 (WASM Package), 02-003 (Offline), 02-004 (Tool Approval), 02-005 (Error Handling), 02-006 (Token Tracking) are independent
- 02-007 (WASM Sample) requires 02-002

**Within Phase 03**:
- 03-001 through 03-006 can run in parallel after Phase 02 completes
- 03-007 (BlazorServer.Full) and 03-008 (BlazorWasm.Full) can run in parallel but should be last

## Critical Path

00-001 → 00-002 → 00-003 → 00-004 → 01-005 → 01-002/01-003 → 01-001 → 01-006 → Phase 02 → Phase 03

## Notes

- **Total Epics**: 25 greenlit
- **Total Phases**: 4 (Phase 00 through Phase 03)
- **Estimated Duration**:
  - Phase 00: 2-3 weeks (foundation work)
  - Phase 01: 3-4 weeks (MVP implementation)
  - Phase 02: 3-4 weeks (production hardening)
  - Phase 03: 3-4 weeks (advanced features)
  - **Total Sequential**: 11-15 weeks
  - **Total with Parallelization**: 7-10 weeks
- **Last Updated**: 2025-12-11

**Priority Focus**:
- Phase 00 epics are foundational and MUST be completed first
- 00-001 (Repository Structure) is the absolute first epic
- 00-002 (Core Abstractions) defines interfaces for all components
- Complete Phase 00 and Phase 01 for functional MVP
