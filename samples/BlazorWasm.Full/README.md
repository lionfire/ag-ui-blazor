# BlazorWasm.Full Sample

A comprehensive Blazor WebAssembly sample application demonstrating all WASM-specific features of the AG-UI Blazor library including offline support and PWA capabilities.

## Features Demonstrated

### WASM-Specific Features
- **Offline Message Queue** - Messages sent while offline are queued locally
- **Auto-Reconnection** - Automatic reconnection with exponential backoff
- **Connection Monitoring** - Real-time connection state tracking
- **PWA Support** - Installable as a Progressive Web App
- **Service Worker** - Offline asset caching for production

### Chat Components
- **MudAgentChat** - Full-featured AI chat interface
- **MudMessageInput** - Customizable message input

### Multi-Agent Support
- Multiple agent configurations (assistant, coder, researcher)
- Tab-based agent switching
- Agent-specific chat personalities

## Pages

| Page | Route | Description |
|------|-------|-------------|
| Home | `/` | Feature overview and navigation |
| Chat Demo | `/chat` | Basic chat with streaming responses |
| Multi-Agent | `/multiagent` | Multiple agents with tab switching |
| Offline Mode | `/offline` | Offline queue demonstration |
| Network Status | `/network` | Connection monitoring dashboard |

## Running the Sample

```bash
cd samples/BlazorWasm.Full/Server
dotnet run
```

The application will be available at the default URLs (typically https://localhost:5001).

## Project Structure

```
BlazorWasm.Full/
├── Client/                         # Blazor WebAssembly client
│   ├── Layout/
│   │   ├── MainLayout.razor        # Main application layout
│   │   ├── ConnectionStatusIndicator.razor
│   │   └── NetworkStatusIndicator.razor
│   ├── Pages/
│   │   ├── Index.razor             # Home page with feature cards
│   │   ├── ChatDemo.razor          # Basic chat demonstration
│   │   ├── MultiAgent.razor        # Multi-agent tab interface
│   │   ├── Offline.razor           # Offline mode demonstration
│   │   └── NetworkStatus.razor     # Connection monitoring
│   ├── wwwroot/
│   │   ├── css/app.css             # Application styles
│   │   ├── manifest.json           # PWA manifest
│   │   ├── service-worker.js       # Development service worker
│   │   └── service-worker.published.js  # Production service worker
│   ├── App.razor                   # Root component
│   ├── Program.cs                  # WASM entry point
│   └── _Imports.razor              # Global imports
├── Server/                         # ASP.NET Core host
│   ├── MockChatClient.cs           # Mock AI client with personalities
│   └── Program.cs                  # Server entry point with API endpoints
└── Shared/                         # Shared models
    └── AgentInfo.cs                # Agent information model
```

## Offline Mode

The sample demonstrates offline capabilities:

1. **Message Queueing**: When offline, messages are stored in browser LocalStorage
2. **Queue Persistence**: Messages survive page reloads
3. **Auto-Retry**: When connection is restored, queued messages are sent automatically
4. **Exponential Backoff**: Reconnection attempts use exponential backoff

### Testing Offline Mode

1. Navigate to the Offline Mode page (`/offline`)
2. Open browser DevTools > Network tab
3. Set network to "Offline"
4. Send messages - they will be queued
5. Restore network - messages will be sent

## PWA Installation

The application can be installed as a Progressive Web App:

1. Visit the application in Chrome/Edge
2. Click the install icon in the address bar
3. The app can run independently with offline asset caching

## Mock Chat Client

The server includes a `MockChatClient` that simulates different AI personalities:

- **Assistant** - Helpful general-purpose assistant
- **Coder** - Programming-focused responses
- **Researcher** - Research and analysis style

## Dependencies

- **LionFire.AgUi.Blazor** - Core AG-UI abstractions
- **LionFire.AgUi.Blazor.Wasm** - WASM-specific services (offline queue, connection monitor)
- **LionFire.AgUi.Blazor.MudBlazor** - MudBlazor UI components
- **MudBlazor** - Material Design component library

## Configuration

Client configuration in `Program.cs`:

```csharp
builder.Services
    .AddAgUiBlazorWasm(options =>
    {
        options.ServerUrl = builder.HostEnvironment.BaseAddress;
        options.EnableAutoReconnect = true;
        options.EnableOfflineQueue = true;
        options.MaxQueuedMessages = 100;
        options.ReconnectDelay = TimeSpan.FromSeconds(5);
    })
    .RegisterWasmAgent("assistant", "Demo AI Assistant")
    .RegisterWasmAgent("coder", "Coding Assistant")
    .RegisterWasmAgent("researcher", "Research Assistant");
```

## License

This sample is part of the AG-UI Blazor library. See the main repository for license information.
