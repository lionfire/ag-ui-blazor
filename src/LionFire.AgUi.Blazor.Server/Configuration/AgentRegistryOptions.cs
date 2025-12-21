using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Server.Configuration;

/// <summary>
/// Configuration options for the agent registry.
/// </summary>
/// <remarks>
/// This class maintains a collection of registered agents that can be retrieved by the
/// <see cref="Services.DirectAgentClientFactory"/>. Agents are registered via the
/// service collection extension methods.
/// </remarks>
public sealed class AgentRegistryOptions
{
    /// <summary>
    /// Gets the dictionary of registered agents, keyed by agent name.
    /// </summary>
    /// <remarks>
    /// Agent names are case-sensitive. Each agent entry contains metadata
    /// about the agent including its description and availability status.
    /// </remarks>
    public Dictionary<string, AgentInfo> Agents { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers an agent with the specified metadata.
    /// </summary>
    /// <param name="name">The unique name of the agent.</param>
    /// <param name="description">An optional description of the agent's capabilities.</param>
    /// <param name="iconUrl">An optional URL to an icon representing the agent.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an agent with the same name is already registered.</exception>
    public void RegisterAgent(string name, string? description = null, string? iconUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (Agents.ContainsKey(name))
        {
            throw new InvalidOperationException($"An agent with the name '{name}' is already registered.");
        }

        Agents[name] = new AgentInfo(name, description, iconUrl, IsAvailable: true);
    }
}
