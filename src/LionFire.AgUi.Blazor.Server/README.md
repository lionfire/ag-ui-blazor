# LionFire.AgUi.Blazor.Server

Blazor Server optimizations for AG-UI with direct streaming. Provides sub-millisecond latency by bypassing HTTP entirely.

## Installation

```bash
dotnet add package LionFire.AgUi.Blazor.Server
```

## Overview

This package provides Blazor Server-specific optimizations for AG-UI components. It takes advantage of the persistent SignalR connection to enable direct streaming of AI responses without HTTP overhead.

## Features

- Direct streaming via SignalR (no HTTP round-trips)
- Sub-millisecond latency for real-time updates
- Server-side caching for conversation state
- Optimized memory management for long conversations
- Built-in connection resilience

## Quick Start

1. Add services in `Program.cs`:

```csharp
using LionFire.AgUi.Blazor.Server;

builder.Services.AddAgUiBlazorServer();
```

2. Configure the middleware:

```csharp
app.UseAgUiBlazorServer();
```

## When to Use

Use this package when:

- Building Blazor Server applications
- Low latency is critical for your use case
- You want to leverage server-side resources for AI processing
- Network conditions are stable (internal apps, enterprise scenarios)

For WebAssembly applications, use [LionFire.AgUi.Blazor.Wasm](https://www.nuget.org/packages/LionFire.AgUi.Blazor.Wasm) instead.

## Documentation

For full documentation, visit the [GitHub repository](https://github.com/LionFire/LionFire.AgUi.Blazor).

## Dependencies

- [LionFire.AgUi.Blazor](https://www.nuget.org/packages/LionFire.AgUi.Blazor)

## License

MIT License - see [LICENSE](https://github.com/LionFire/LionFire.AgUi.Blazor/blob/main/LICENSE) for details.
