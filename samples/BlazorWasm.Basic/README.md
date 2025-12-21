# BlazorWasm.Basic Sample

A Blazor WebAssembly sample demonstrating AG-UI Blazor components with offline support and automatic reconnection.

## Architecture

This sample consists of three projects:

- **Server**: ASP.NET Core backend with AG-UI API endpoints
- **Client**: Blazor WebAssembly frontend with MudBlazor UI
- **Shared**: Common models shared between server and client

## Quick Start

1. Run the server project:

```bash
cd samples/BlazorWasm.Basic/Server
dotnet run
```

2. Open your browser to: `https://localhost:7000` (or the URL shown in console)

## Features Demonstrated

### AG-UI Protocol

The server exposes the AG-UI HTTP API:
- `GET /api/agents` - List available AI agents
- `POST /api/agents/{name}/chat` - Non-streaming chat
- `POST /api/agents/{name}/chat/stream` - Streaming chat (SSE)

### Client Features

- **MudAgentChat**: Full-featured chat component with MudBlazor styling
- **Connection Status**: Visual indicator showing online/offline state
- **Dark Mode**: System preference detection with manual toggle
- **Offline Queue**: Messages are queued when offline and sent when reconnected

## Testing Offline Behavior

1. Start the application
2. Send a message and verify it works
3. Open browser DevTools > Network tab
4. Toggle "Offline" mode in DevTools
5. Send another message - it should be queued
6. Toggle "Offline" off
7. The queued message should be sent automatically

## Project Structure

```
BlazorWasm.Basic/
  Client/
    Layout/
      MainLayout.razor         # App shell with theme support
      ConnectionStatusIndicator.razor  # Online/offline indicator
    Pages/
      Index.razor              # Main chat page
    wwwroot/
      index.html               # HTML entry point
    Program.cs                 # Client DI configuration
    App.razor                  # Blazor app root
    _Imports.razor             # Global using statements
  Server/
    Program.cs                 # Server API configuration
    MockChatClient.cs          # Demo IChatClient implementation
  Shared/
    AgentInfo.cs               # Shared models
```

## Configuration

### Client

The client is configured in `Program.cs`:

```csharp
builder.Services.AddAgUiBlazorWasm(options =>
{
    options.ServerUrl = builder.HostEnvironment.BaseAddress;
    options.EnableAutoReconnect = true;
    options.EnableOfflineQueue = true;
});
```

### Server

The server uses the mock chat client for demo purposes. Replace with a real IChatClient for production:

```csharp
// Replace MockChatClient with your implementation
builder.Services.AddSingleton<IChatClient, YourChatClient>();
```

## Using a Real AI Service

To connect to a real AI service:

1. Install the appropriate Microsoft.Extensions.AI implementation package
2. Configure your API key in secrets or environment variables
3. Replace the MockChatClient registration in Server/Program.cs

Example with OpenAI:

```csharp
builder.Services.AddSingleton<IChatClient>(sp =>
    new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
        .AsChatClient("gpt-4"));
```
