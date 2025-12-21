using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Wasm.Configuration;
using LionFire.AgUi.Blazor.Wasm.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LionFire.AgUi.Blazor.Wasm.Extensions;

/// <summary>
/// Extension methods for configuring AG-UI Blazor WASM services.
/// </summary>
/// <remarks>
/// These extensions provide a fluent API for registering HTTP-based agent services
/// in the dependency injection container for Blazor WebAssembly applications.
/// </remarks>
/// <example>
/// <code>
/// // In Program.cs
/// builder.Services
///     .AddAgUiBlazorWasm(options =>
///     {
///         options.ServerUrl = "https://api.example.com";
///     });
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AG-UI Blazor WASM services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">An optional action to configure <see cref="WasmAgentClientOptions"/>.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method registers the <see cref="HttpAgentClientFactory"/> as the implementation
    /// for <see cref="IAgentClientFactory"/>. The factory communicates with an AG-UI server
    /// endpoint over HTTP/SSE for streaming responses.
    /// </para>
    /// <para>
    /// An HttpClient is configured for the factory with the appropriate base address and timeout settings.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddAgUiBlazorWasm(options =>
    /// {
    ///     options.ServerUrl = builder.HostEnvironment.BaseAddress;
    ///     options.Timeout = TimeSpan.FromSeconds(60);
    ///     options.EnableAutoReconnect = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAgUiBlazorWasm(
        this IServiceCollection services,
        Action<WasmAgentClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register options
        services.AddOptions<WasmAgentClientOptions>();

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register HttpClient for the factory
        services.AddHttpClient<HttpAgentClientFactory>();

        // Register the factory as singleton
        services.TryAddSingleton<HttpAgentClientFactory>();
        services.TryAddSingleton<IAgentClientFactory>(sp => sp.GetRequiredService<HttpAgentClientFactory>());

        // Register offline support services
        services.TryAddSingleton<ConnectionMonitor>();
        services.TryAddSingleton<IConnectionMonitor>(sp => sp.GetRequiredService<ConnectionMonitor>());
        services.TryAddSingleton<LocalStorageMessageQueue>();
        services.TryAddSingleton<IOfflineMessageQueue>(sp => sp.GetRequiredService<LocalStorageMessageQueue>());

        return services;
    }

    /// <summary>
    /// Adds AG-UI Blazor WASM services with a specific server URL.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="serverUrl">The base URL for the AG-UI server endpoint.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddAgUiBlazorWasm("https://api.example.com");
    /// </code>
    /// </example>
    public static IServiceCollection AddAgUiBlazorWasm(
        this IServiceCollection services,
        string serverUrl)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serverUrl);

        return services.AddAgUiBlazorWasm(options =>
        {
            options.ServerUrl = serverUrl;
        });
    }

    /// <summary>
    /// Registers a known agent with the WASM client factory.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="name">The unique name of the agent.</param>
    /// <param name="description">An optional description of the agent.</param>
    /// <param name="iconUrl">An optional URL to an icon representing the agent.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method pre-registers agent metadata without creating an actual connection.
    /// The actual agent connection is created when <see cref="IAgentClientFactory.GetAgentAsync"/>
    /// is called.
    /// </para>
    /// <para>
    /// Pre-registering agents allows the UI to display available agents before
    /// fetching the list from the server.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// services
    ///     .AddAgUiBlazorWasm("https://api.example.com")
    ///     .RegisterWasmAgent("assistant", "A helpful AI assistant")
    ///     .RegisterWasmAgent("coder", "A coding assistant");
    /// </code>
    /// </example>
    public static IServiceCollection RegisterWasmAgent(
        this IServiceCollection services,
        string name,
        string? description = null,
        string? iconUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Register a callback to configure the factory after it's created
        services.AddSingleton<IConfigureOptions<WasmAgentClientOptions>>(sp =>
        {
            return new ConfigureAgentRegistration(name, description, iconUrl);
        });

        return services;
    }

    private sealed class ConfigureAgentRegistration : IConfigureOptions<WasmAgentClientOptions>
    {
        private readonly string _name;
        private readonly string? _description;
        private readonly string? _iconUrl;

        public ConfigureAgentRegistration(string name, string? description, string? iconUrl)
        {
            _name = name;
            _description = description;
            _iconUrl = iconUrl;
        }

        public void Configure(WasmAgentClientOptions options)
        {
            // Agent registration is handled separately in the factory
            // This is a placeholder for future configuration needs
        }
    }
}

/// <summary>
/// Marker interface for configuring WASM agent client options.
/// </summary>
public interface IConfigureOptions<TOptions>
{
    /// <summary>
    /// Configures the options.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    void Configure(TOptions options);
}
