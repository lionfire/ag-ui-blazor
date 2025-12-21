---
greenlit: true
status: done
---

# Epic 03-006: Keyboard Shortcuts

**Phase**: 03 - Advanced Features
**Estimated Effort**: 2-3 days
**Status**: DONE

## Overview
Implement keyboard shortcuts for common actions (send, new chat, etc.).

**Link to Phase**: [Phase 03: Advanced Features](../../40-phases/03-advanced-features.md)

## Implementation Tasks
- [x] Implement keyboard shortcut service (IKeyboardShortcutService, KeyboardShortcutService)
- [x] Add shortcuts: Ctrl+Enter (send), Ctrl+K (new chat), Ctrl+/ (shortcuts help)
- [x] Add Escape (cancel streaming/close modals)
- [x] Add arrow keys (navigate messages)
- [x] Add shortcuts to MudAgentChat (HandleKeyDown handler)
- [x] Create keyboard shortcuts help dialog (MudKeyboardShortcutsHelp)
- [x] Make shortcuts configurable (Register/Unregister methods)
- [x] Handle platform differences (Ctrl vs Cmd on Mac) via SetPlatform and IsMac
- [x] Prevent conflicts with browser shortcuts (uses modifier keys)
- [x] Document shortcuts in UI (help dialog with category grouping)

## Implementation Details

### New Files Created
- `src/LionFire.AgUi.Blazor/Models/KeyboardShortcut.cs` - Keyboard shortcut model and KeyModifiers enum
- `src/LionFire.AgUi.Blazor/Abstractions/IKeyboardShortcutService.cs` - Service interface
- `src/LionFire.AgUi.Blazor/Services/KeyboardShortcutService.cs` - Service implementation
- `src/LionFire.AgUi.Blazor.MudBlazor/Components/MudKeyboardShortcutsHelp.razor*` - Help dialog component

### Default Shortcuts
- **Messages**: Ctrl+Enter (send message when not in input)
- **Chat**: Ctrl+K (new chat), Escape (cancel streaming/close dialog)
- **Navigation**: Ctrl+Arrow Up/Down (navigate messages)
- **Help**: Ctrl+/ or Ctrl+Shift+? (show shortcuts)

### Features
- Platform-aware display strings (Ctrl on Windows, ⌘ on Mac)
- Category-based organization in help dialog
- Configurable shortcuts via service
- Cross-platform modifier key handling

## Acceptance Criteria
- [x] All shortcuts work correctly
- [x] No conflicts with browser (modifier keys used)
- [x] Help dialog shows shortcuts (MudKeyboardShortcutsHelp)
- [x] Platform differences handled (IsMac property, GetDisplayString)
