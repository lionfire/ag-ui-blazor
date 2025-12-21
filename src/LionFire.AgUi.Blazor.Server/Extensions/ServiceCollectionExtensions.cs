using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Configuration;
using LionFire.AgUi.Blazor.Server.Configuration;
using LionFire.AgUi.Blazor.Server.Services;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LionFire.AgUi.Blazor.Server.Extensions;

/// <summary>
/// Extension methods for configuring AG-UI Blazor Server services.
/// </summary>
/// <remarks>
/// These extensions provide a fluent API for registering AI agents and the
/// <see cref="DirectAgentClientFactory"/> in the dependency injection container.
/// </remarks>
/// <example>
/// <code>
/// // In Program.cs
/// builder.Services
///     .AddAgUiBlazorServer()
///     .AddAgent&lt;MyAssistant&gt;("assistant", "A helpful AI assistant")
///     .AddAgent("chatbot", sp => new ChatBotAgent(sp.GetRequiredService&lt;IConfiguration&gt;()));
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AG-UI Blazor Server services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">An optional action to configure <see cref="AgentRegistryOptions"/>.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method registers the <see cref="DirectAgentClientFactory"/> as the implementation
    /// for <see cref="IAgentClientFactory"/>. The factory is registered as a singleton to
    /// allow sharing of the agent registry across requests.
    /// </para>
    /// <para>
    /// Call this method before registering any agents with AddAgent methods.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddAgUiBlazorServer(options =>
    /// {
    ///     // Pre-register agents via options if needed
    ///     options.RegisterAgent("default", "Default assistant");
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAgUiBlazorServer(
        this IServiceCollection services,
        Action<AgentRegistryOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Always register options through the options system to support later Configure calls
        services.AddOptions<AgentRegistryOptions>();

        // Apply initial configuration if provided
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register the factory as singleton (agent registry is immutable after startup)
        services.TryAddSingleton<DirectAgentClientFactory>();
        services.TryAddSingleton<IAgentClientFactory>(sp => sp.GetRequiredService<DirectAgentClientFactory>());

        // Register state manager options and default implementation
        services.AddOptions<StateManagerOptions>();
        services.TryAddSingleton<InMemoryStateManager>();
        services.TryAddSingleton<IAgentStateManager>(sp => sp.GetRequiredService<InMemoryStateManager>());

        return services;
    }

    /// <summary>
    /// Configures the state manager options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">An action to configure <see cref="StateManagerOptions"/>.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// services
    ///     .AddAgUiBlazorServer()
    ///     .ConfigureStateManager(options =>
    ///     {
    ///         options.MaxConversations = 50;
    ///         options.MaxConversationAge = TimeSpan.FromDays(7);
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureStateManager(
        this IServiceCollection services,
        Action<StateManagerOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Registers an agent type with the specified name.
    /// </summary>
    /// <typeparam name="TAgent">The agent type implementing <see cref="IChatClient"/>.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="name">The unique name to identify the agent.</param>
    /// <param name="description">An optional description of the agent's capabilities.</param>
    /// <param name="iconUrl">An optional URL to an icon representing the agent.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>
    /// The agent is registered as a keyed singleton service, allowing retrieval by name
    /// via <see cref="IAgentClientFactory.GetAgentAsync"/>.
    /// </para>
    /// <para>
    /// Ensure <see cref="AddAgUiBlazorServer"/> is called before this method to set up
    /// the required infrastructure.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// services
    ///     .AddAgUiBlazorServer()
    ///     .AddAgent&lt;OpenAIChatClient&gt;("gpt4", "GPT-4 powered assistant");
    /// </code>
    /// </example>
    public static IServiceCollection AddAgent<TAgent>(
        this IServiceCollection services,
        string name,
        string? description = null,
        string? iconUrl = null)
        where TAgent : class, IChatClient
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        // Register the agent as a keyed singleton
        services.AddKeyedSingleton<IChatClient, TAgent>(name);

        // Register in the options
        services.Configure<AgentRegistryOptions>(options =>
        {
            options.RegisterAgent(name, description, iconUrl);
        });

        return services;
    }

    /// <summary>
    /// Registers an agent using a factory function.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="name">The unique name to identify the agent.</param>
    /// <param name="factory">A factory function that creates the agent instance.</param>
    /// <param name="description">An optional description of the agent's capabilities.</param>
    /// <param name="iconUrl">An optional URL to an icon representing the agent.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="factory"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>
    /// This overload allows custom initialization logic when creating agents, useful when
    /// agents require configuration values or other dependencies not available via standard DI.
    /// </para>
    /// <para>
    /// The factory is invoked once and the resulting agent is cached as a singleton.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddAgent("claude", sp =>
    /// {
    ///     var config = sp.GetRequiredService&lt;IConfiguration&gt;();
    ///     var apiKey = config["Anthropic:ApiKey"];
    ///     return new AnthropicClient(apiKey);
    /// }, "Claude AI assistant");
    /// </code>
    /// </example>
    public static IServiceCollection AddAgent(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, IChatClient> factory,
        string? description = null,
        string? iconUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(factory);

        // Register the agent as a keyed singleton with factory
        services.AddKeyedSingleton<IChatClient>(name, (sp, _) => factory(sp));

        // Register in the options
        services.Configure<AgentRegistryOptions>(options =>
        {
            options.RegisterAgent(name, description, iconUrl);
        });

        return services;
    }

    /// <summary>
    /// Registers an existing agent instance with the specified name.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="name">The unique name to identify the agent.</param>
    /// <param name="instance">The agent instance to register.</param>
    /// <param name="description">An optional description of the agent's capabilities.</param>
    /// <param name="iconUrl">An optional URL to an icon representing the agent.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="instance"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>
    /// Use this overload when you have a pre-configured agent instance that should be
    /// shared across the application. The caller is responsible for the lifetime of
    /// the instance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var myAgent = new PreConfiguredAgent();
    /// services.AddAgent("preconfigured", myAgent, "A pre-configured agent");
    /// </code>
    /// </example>
    public static IServiceCollection AddAgent(
        this IServiceCollection services,
        string name,
        IChatClient instance,
        string? description = null,
        string? iconUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(instance);

        // Register the agent instance as a keyed singleton
        services.AddKeyedSingleton(name, instance);

        // Register in the options
        services.Configure<AgentRegistryOptions>(options =>
        {
            options.RegisterAgent(name, description, iconUrl);
        });

        return services;
    }
}
