using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWasm.Basic.Client;
using LionFire.AgUi.Blazor.Wasm.Extensions;
using LionFire.AgUi.Blazor.MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add MudBlazor services
builder.Services.AddMudServices();

// Add AG-UI MudBlazor services
builder.Services.AddAgUiMudBlazor();

// Add AG-UI Blazor WASM services with server URL
builder.Services
    .AddAgUiBlazorWasm(options =>
    {
        options.ServerUrl = builder.HostEnvironment.BaseAddress;
        options.EnableAutoReconnect = true;
        options.EnableOfflineQueue = true;
    })
    .RegisterWasmAgent("assistant", "Demo AI Assistant");

await builder.Build().RunAsync();
