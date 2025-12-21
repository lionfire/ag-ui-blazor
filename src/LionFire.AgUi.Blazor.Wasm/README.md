# LionFire.AgUi.Blazor.Wasm

Blazor WebAssembly client for AG-UI with offline support. Provides robust networking with automatic reconnection and request queuing.

## Installation

```bash
dotnet add package LionFire.AgUi.Blazor.Wasm
```

## Overview

This package provides WebAssembly-specific features for AG-UI components, including HTTP-based communication with AG-UI servers, offline support, and client-side optimizations.

## Features

- HTTP/SSE-based streaming for AI responses
- Automatic reconnection with exponential backoff
- Request queuing during network interruptions
- Local storage for conversation persistence
- Optimized for WebAssembly runtime constraints
- PWA-ready with offline support

## Quick Start

1. Add services in `Program.cs`:

```csharp
using LionFire.AgUi.Blazor.Wasm;

builder.Services.AddAgUiBlazorWasm(options =>
{
    options.ApiBaseUrl = "https://your-api-server.com";
});
```

2. Configure the HTTP client:

```csharp
builder.Services.AddHttpClient("AgUi", client =>
{
    client.BaseAddress = new Uri("https://your-api-server.com");
});
```

## When to Use

Use this package when:

- Building Blazor WebAssembly applications
- You need offline capabilities
- Users may have unreliable network connections
- You want client-side execution (no server required after initial load)

For Blazor Server applications, use [LionFire.AgUi.Blazor.Server](https://www.nuget.org/packages/LionFire.AgUi.Blazor.Server) instead.

## Documentation

For full documentation, visit the [GitHub repository](https://github.com/LionFire/LionFire.AgUi.Blazor).

## Dependencies

- [LionFire.AgUi.Blazor](https://www.nuget.org/packages/LionFire.AgUi.Blazor)

## License

MIT License - see [LICENSE](https://github.com/LionFire/LionFire.AgUi.Blazor/blob/main/LICENSE) for details.
