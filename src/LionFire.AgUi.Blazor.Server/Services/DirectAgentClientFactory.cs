using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Server.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.AgUi.Blazor.Server.Services;

/// <summary>
/// Factory for obtaining agent instances directly from the dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// This implementation provides direct in-process streaming from agents without HTTP overhead,
/// achieving sub-millisecond latency. It is designed for Blazor Server scenarios where the
/// UI runs in the same process as the agents.
/// </para>
/// <para>
/// Agents are registered using keyed services in .NET 8+ and retrieved by name. The factory
/// maintains a cache of agent metadata for efficient listing operations.
/// </para>
/// <para>
/// Since this is a direct in-process connection, <see cref="GetConnectionState"/> always
/// returns <see cref="ConnectionState.Connected"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register agents in Program.cs or Startup
/// services.AddAgUiBlazorServer()
///     .AddAgent&lt;MyCustomAgent&gt;("assistant", "A helpful assistant");
///
/// // Use in a Blazor component
/// @inject IAgentClientFactory AgentFactory
///
/// var agent = await AgentFactory.GetAgentAsync("assistant");
/// if (agent != null)
/// {
///     await foreach (var update in agent.CompleteStreamingAsync(...))
///     {
///         // Handle streaming response
///     }
/// }
/// </code>
/// </example>
public sealed class DirectAgentClientFactory : IAgentClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<AgentInfo> _registeredAgents;
    private readonly ILogger<DirectAgentClientFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectAgentClientFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving agents.</param>
    /// <param name="options">The agent registry configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serviceProvider"/>, <paramref name="options"/>, or <paramref name="logger"/> is null.
    /// </exception>
    public DirectAgentClientFactory(
        IServiceProvider serviceProvider,
        IOptions<AgentRegistryOptions> options,
        ILogger<DirectAgentClientFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (options?.Value == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        // Create immutable snapshot of registered agents for thread-safe access
        _registeredAgents = options.Value.Agents.Values.ToList().AsReadOnly();

        _logger.LogDebug("DirectAgentClientFactory initialized with {AgentCount} registered agents", _registeredAgents.Count);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Retrieves an agent from the DI container using keyed services.
    /// Returns <c>null</c> if the agent is not found, allowing callers to handle missing agents gracefully.
    /// </remarks>
    public Task<IChatClient?> GetAgentAsync(string agentName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName, nameof(agentName));

        ct.ThrowIfCancellationRequested();

        _logger.LogDebug("Attempting to retrieve agent '{AgentName}'", agentName);

        // Try to resolve using keyed services (introduced in .NET 8)
        var agent = _serviceProvider.GetKeyedService<IChatClient>(agentName);

        if (agent == null)
        {
            _logger.LogWarning(
                "Agent '{AgentName}' was not found. Available agents: {AvailableAgents}",
                agentName,
                string.Join(", ", _registeredAgents.Select(a => a.Name)));

            return Task.FromResult<IChatClient?>(null);
        }

        _logger.LogDebug("Successfully retrieved agent '{AgentName}'", agentName);
        return Task.FromResult<IChatClient?>(agent);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AgentInfo>> ListAgentsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogDebug("Listing {AgentCount} registered agents", _registeredAgents.Count);
        return Task.FromResult(_registeredAgents);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// For direct in-process streaming, the connection is always considered active
    /// since there is no network layer involved.
    /// </remarks>
    public ConnectionState GetConnectionState()
    {
        // Direct streaming has no network connection - always "connected"
        return ConnectionState.Connected;
    }
}
