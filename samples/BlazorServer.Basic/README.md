# BlazorServer.Basic Sample

A minimal Blazor Server sample demonstrating the MudAgentChat component with a mock AI agent.

## Prerequisites

- .NET 9.0 SDK or later
- No API keys or configuration required

## Running the Sample

From this directory:

```bash
dotnet run
```

Or from the solution root:

```bash
dotnet run --project samples/BlazorServer.Basic
```

The application will start on `https://localhost:5001` (or the port shown in the console).

## What This Sample Demonstrates

1. **MudAgentChat Component**: A full-featured chat UI with streaming support
2. **Blazor Server Integration**: Real-time updates via SignalR with sub-millisecond latency
3. **MudBlazor Theming**: Dark/light mode toggle with system preference detection
4. **Mock AI Agent**: Demonstrates the IChatClient interface without external dependencies

## Project Structure

```
BlazorServer.Basic/
  Components/
    App.razor           - Root component with MudBlazor resources
    _Imports.razor      - Global using statements
    Layout/
      MainLayout.razor  - MudBlazor layout with theme switching
    Pages/
      Home.razor        - Chat page using MudAgentChat
  MockChatClient.cs     - Sample IChatClient implementation
  Program.cs            - Service configuration
```

## Key Code Snippets

### Program.cs Setup

```csharp
// Add MudBlazor services
builder.Services.AddMudServices();

// Add AG-UI Blazor Server with a mock agent
builder.Services
    .AddAgUiBlazorServer()
    .AddAgent("assistant", _ => new MockChatClient(), "Demo AI Assistant");
```

### Using the Chat Component

```razor
@using LionFire.AgUi.Blazor.MudBlazor.Components

<MudAgentChat AgentName="assistant" />
```

## Customization

### Using a Real AI Service

Replace the mock client with a real AI service:

```csharp
// Example with OpenAI (requires Microsoft.Extensions.AI.OpenAI package)
builder.Services
    .AddAgUiBlazorServer()
    .AddAgent("assistant", sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new OpenAIChatClient("gpt-4", config["OpenAI:ApiKey"]);
    }, "GPT-4 Assistant");
```

### Adding Multiple Agents

```csharp
builder.Services
    .AddAgUiBlazorServer()
    .AddAgent("gpt4", _ => new OpenAIChatClient("gpt-4", apiKey), "GPT-4")
    .AddAgent("claude", _ => new AnthropicClient(anthropicKey), "Claude");
```

### Custom Styling

The MudAgentChat component respects MudBlazor theming. Customize via:

```razor
<MudThemeProvider Theme="@_customTheme" />

@code {
    private MudTheme _customTheme = new()
    {
        PrimaryColor = "#1976D2",
        // ... other customizations
    };
}
```

## Troubleshooting

### Component Not Rendering

Ensure the render mode is set to Interactive Server:

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

### Streaming Not Working

Check that the agent is properly registered and the MudAgentChat AgentName matches:

```razor
<!-- Must match the name used in AddAgent() -->
<MudAgentChat AgentName="assistant" />
```

## Learn More

- [MudBlazor Documentation](https://mudblazor.com/)
- [Blazor Server Documentation](https://docs.microsoft.com/aspnet/core/blazor/hosting-models#blazor-server)
- [Microsoft.Extensions.AI](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/)
