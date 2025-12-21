# LionFire.AgUi.Blazor.MudBlazor

Rich MudBlazor components for AI agent chat interfaces. Provides beautiful, production-ready chat UI with full MudBlazor theming support.

## Installation

```bash
dotnet add package LionFire.AgUi.Blazor.MudBlazor
```

## Overview

This package provides ready-to-use MudBlazor components for building AI chat interfaces. It includes chat bubbles, message lists, input components, and more, all styled with MudBlazor's Material Design theme.

## Features

- Pre-built chat UI components using MudBlazor
- Full MudBlazor theming support (light/dark modes)
- Responsive design for mobile and desktop
- Markdown rendering support
- Code syntax highlighting
- Streaming message support with typing indicators

## Quick Start

1. Add MudBlazor services in `Program.cs`:

```csharp
using LionFire.AgUi.Blazor.MudBlazor;
using MudBlazor.Services;

builder.Services.AddMudServices();
builder.Services.AddAgUiMudBlazor();
```

2. Add MudBlazor imports to `_Imports.razor`:

```razor
@using MudBlazor
@using LionFire.AgUi.Blazor.MudBlazor
```

3. Use the chat component:

```razor
<AgUiChat />
```

## Documentation

For full documentation, visit the [GitHub repository](https://github.com/LionFire/LionFire.AgUi.Blazor).

## Dependencies

- [MudBlazor](https://mudblazor.com/) 8.x
- [LionFire.AgUi.Blazor](https://www.nuget.org/packages/LionFire.AgUi.Blazor)

## License

MIT License - see [LICENSE](https://github.com/LionFire/LionFire.AgUi.Blazor/blob/main/LICENSE) for details.
