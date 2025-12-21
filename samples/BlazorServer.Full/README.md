# BlazorServer.Full Sample

A comprehensive Blazor Server sample application demonstrating all features of the AG-UI Blazor library.

## Features Demonstrated

### Chat Components
- **MudAgentChat** - Full-featured AI chat interface with message operations
- **MudAgentChatWithHistory** - Chat with conversation history sidebar
- **MudMessageInput** - Customizable message input with keyboard shortcuts

### Multi-Agent Support
- Multiple agent configurations (assistant, coder, researcher)
- Tab-based agent switching
- Agent-specific chat personalities

### Developer Tools
- **MudEventLog** - Real-time event logging for debugging
- **MudStateViewer** - State inspection and visualization

### Conversation Management
- Create new conversations
- Conversation history browsing
- Message editing and operations

### Keyboard Shortcuts
- `Ctrl+Enter` - Send message
- `Ctrl+K` - New chat
- `Escape` - Cancel editing
- `Ctrl+/` - Show shortcuts help

## Pages

| Page | Route | Description |
|------|-------|-------------|
| Home | `/` | Feature overview and navigation |
| Chat Demo | `/chat` | Basic chat with event callbacks |
| Multi-Agent | `/multiagent` | Multiple agents with tab switching |
| History | `/history` | Chat with conversation history sidebar |
| Developer | `/developer` | Event log and state viewer demo |

## Running the Sample

```bash
cd samples/BlazorServer.Full
dotnet run
```

The application will be available at:
- HTTPS: https://localhost:5101
- HTTP: http://localhost:5100

## Project Structure

```
BlazorServer.Full/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor    # Main application layout
│   ├── Pages/
│   │   ├── Home.razor          # Home page with feature cards
│   │   ├── ChatDemo.razor      # Basic chat demonstration
│   │   ├── MultiAgent.razor    # Multi-agent tab interface
│   │   ├── History.razor       # Chat with history sidebar
│   │   ├── Developer.razor     # Developer tools demo
│   │   └── Error.razor         # Error page
│   ├── App.razor               # Root component
│   ├── Routes.razor            # Router configuration
│   └── _Imports.razor          # Global imports
├── wwwroot/
│   ├── app.css                 # Application styles
│   └── favicon.png             # Application icon
├── MockChatClient.cs           # Mock AI client with personalities
├── Program.cs                  # Application entry point
├── appsettings.json            # Configuration
└── BlazorServer.Full.csproj    # Project file
```

## Mock Chat Client

The sample includes a `MockChatClient` that simulates different AI personalities:

- **Assistant** - Helpful general-purpose assistant
- **Coder** - Programming-focused responses
- **Researcher** - Research and analysis style

The mock client provides realistic streaming responses for demonstration purposes.

## Dependencies

- **LionFire.AgUi.Blazor** - Core AG-UI abstractions
- **LionFire.AgUi.Blazor.Server** - Blazor Server integration
- **LionFire.AgUi.Blazor.MudBlazor** - MudBlazor UI components
- **MudBlazor** - Material Design component library

## Configuration

Agent configuration in `Program.cs`:

```csharp
builder.Services
    .AddAgUiBlazorServer()
    .AddAgUiBlazor()
    .AddAgent("assistant", _ => new MockChatClient("assistant"), "Demo AI Assistant")
    .AddAgent("coder", _ => new MockChatClient("coder"), "Coding Assistant")
    .AddAgent("researcher", _ => new MockChatClient("researcher"), "Research Assistant");
```

## License

This sample is part of the AG-UI Blazor library. See the main repository for license information.
