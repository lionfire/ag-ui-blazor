# MudBlazor

## Brief Description

MudBlazor is a Material Design component library for Blazor, providing 60+ components with theming, customization, and responsive design out of the box. It's the primary UI framework for this project.

## Relevance to Project

**Why this matters**:
- Primary UI framework - all components use MudBlazor
- Determines look, feel, and UX patterns
- Provides rich component set (dialogs, snackbars, datagrids, etc.)
- Handles theming and dark mode

**Where it's used**:
- Every UI component inherits MudBlazor patterns
- MudAgentChat, MudMessageList, MudMessageInput - all use MudBlazor
- Theme system defines colors, spacing, typography
- MudDialog for tool approval modals

## Interoperability Points

**Integrates with**:
- Blazor: Built on Blazor component model
- CSS: Uses CSS variables for theming
- JavaScript: Minimal JS interop for advanced features

**Key components used**:
- MudTextField: Text input
- MudButton/MudIconButton: Actions
- MudDialog: Modals
- MudSnackbar: Notifications
- MudDataGrid: Tables
- MudVirtualize: Virtual scrolling
- MudThemeProvider: Theme management

## Considerations

### Best Practices

- Always use MudThemeProvider at root
- Use theme palette colors (Primary, Secondary, etc.)
- Follow Material Design guidelines
- Use MudBlazor CSS utilities (margin, padding, etc.)
- Leverage built-in responsive breakpoints

### Common Pitfalls

- Forgetting MudThemeProvider: Components won't style correctly
- Inline styles instead of theme: Breaks theme switching
- Not using MudBlazor's built-in responsiveness

### Performance

- MudVirtualize for large lists (1000+ items)
- MudBlazor bundle size: ~500 KB (acceptable for rich UI)
- CSS-in-JS can impact first render (negligible for most apps)

## References

- [MudBlazor Documentation](https://mudblazor.com/)
- [MudBlazor GitHub](https://github.com/MudBlazor/MudBlazor)
- [Material Design Guidelines](https://material.io/design)
