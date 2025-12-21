# LionFire.AgUi.Blazor

Core abstractions and interfaces for Blazor AG-UI components. Provides base types for building AI agent chat interfaces in Blazor applications.

## Installation

```bash
dotnet add package LionFire.AgUi.Blazor
```

## Overview

This package provides the foundational types and abstractions for building AG-UI (Agent User Interface) components in Blazor. It is designed to work with Microsoft.Extensions.AI for AI integration.

## Features

- Base interfaces and abstractions for AI chat components
- Message models and conversation state management
- Extensible architecture for custom implementations
- Full .NET 8.0 and .NET 9.0 support

## Basic Usage

```csharp
using LionFire.AgUi.Blazor;

// Register services
services.AddAgUiBlazor();
```

## Documentation

For full documentation, visit the [GitHub repository](https://github.com/LionFire/LionFire.AgUi.Blazor).

## Related Packages

- [LionFire.AgUi.Blazor.MudBlazor](https://www.nuget.org/packages/LionFire.AgUi.Blazor.MudBlazor) - MudBlazor UI components
- [LionFire.AgUi.Blazor.Server](https://www.nuget.org/packages/LionFire.AgUi.Blazor.Server) - Blazor Server optimizations
- [LionFire.AgUi.Blazor.Wasm](https://www.nuget.org/packages/LionFire.AgUi.Blazor.Wasm) - WebAssembly client support
- [LionFire.AgUi.Blazor.Testing](https://www.nuget.org/packages/LionFire.AgUi.Blazor.Testing) - Test utilities

## License

MIT License - see [LICENSE](https://github.com/LionFire/LionFire.AgUi.Blazor/blob/main/LICENSE) for details.
