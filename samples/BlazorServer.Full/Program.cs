using BlazorServer.Full;
using BlazorServer.Full.Components;
using LionFire.AgUi.Blazor.Server.Extensions;
using LionFire.AgUi.Blazor;
using LionFire.AgUi.Blazor.MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor and MudBlazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Add AG-UI MudBlazor services (markdown renderer, etc.)
builder.Services.AddAgUiMudBlazor();

// Add AG-UI Blazor Server with multiple agents
builder.Services
    .AddAgUiBlazorServer()
    .AddAgUiBlazor() // Adds keyboard shortcuts and other core services
    .AddAgent("assistant", _ => new MockChatClient("assistant"), "Demo AI Assistant")
    .AddAgent("coder", _ => new MockChatClient("coder"), "Coding Assistant")
    .AddAgent("researcher", _ => new MockChatClient("researcher"), "Research Assistant");

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
