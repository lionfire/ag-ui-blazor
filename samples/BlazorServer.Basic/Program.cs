using BlazorServer.Basic;
using BlazorServer.Basic.Components;
using LionFire.AgUi.Blazor;
using LionFire.AgUi.Blazor.MudBlazor;
using LionFire.AgUi.Blazor.Server.Extensions;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor and MudBlazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Add AG-UI MudBlazor services (markdown renderer, etc.)
builder.Services.AddAgUiMudBlazor();

// Add AG-UI Blazor Server with a mock agent
builder.Services
    .AddAgUiBlazorServer()
    .AddAgUiBlazor() // Adds keyboard shortcuts and other core services
    .AddAgent("assistant", _ => new MockChatClient(), "Demo AI Assistant");

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
