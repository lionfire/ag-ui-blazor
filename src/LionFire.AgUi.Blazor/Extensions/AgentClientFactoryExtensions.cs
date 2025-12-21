using LionFire.AgUi.Blazor.Abstractions;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Extensions;

/// <summary>
/// Extension methods for <see cref="IAgentClientFactory"/>.
/// </summary>
public static class AgentClientFactoryExtensions
{
    /// <summary>
    /// Gets an agent by name, throwing an exception if not found.
    /// </summary>
    /// <param name="factory">The agent client factory.</param>
    /// <param name="agentName">The name of the agent to retrieve.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>The requested <see cref="IChatClient"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the agent is not found or not available.
    /// </exception>
    /// <remarks>
    /// This method is useful when an agent is required and its absence should be treated as an error.
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     var agent = await factory.GetOrThrowAsync("assistant");
    ///     // Use the agent
    /// }
    /// catch (InvalidOperationException ex)
    /// {
    ///     // Handle missing agent
    ///     logger.LogError(ex, "Required agent not available");
    /// }
    /// </code>
    /// </example>
    public static async Task<IChatClient> GetOrThrowAsync(
        this IAgentClientFactory factory,
        string agentName,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

        var agent = await factory.GetAgentAsync(agentName, ct).ConfigureAwait(false);

        if (agent is null)
        {
            var availableAgents = await factory.ListAgentsAsync(ct).ConfigureAwait(false);
            var availableNames = availableAgents
                .Where(a => a.IsAvailable)
                .Select(a => a.Name)
                .ToList();

            var message = availableNames.Count > 0
                ? $"Agent '{agentName}' not found or not available. Available agents: {string.Join(", ", availableNames)}"
                : $"Agent '{agentName}' not found or not available. No agents are currently available.";

            throw new InvalidOperationException(message);
        }

        return agent;
    }

    /// <summary>
    /// Checks whether an agent is available.
    /// </summary>
    /// <param name="factory">The agent client factory.</param>
    /// <param name="agentName">The name of the agent to check.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// <c>true</c> if the agent exists and is available; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks the agent list without establishing a connection,
    /// making it suitable for UI availability indicators.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (await factory.IsAgentAvailableAsync("assistant"))
    /// {
    ///     // Show the agent option in UI
    /// }
    /// </code>
    /// </example>
    public static async Task<bool> IsAgentAvailableAsync(
        this IAgentClientFactory factory,
        string agentName,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (string.IsNullOrWhiteSpace(agentName))
        {
            return false;
        }

        var agents = await factory.ListAgentsAsync(ct).ConfigureAwait(false);
        return agents.Any(a => a.Name == agentName && a.IsAvailable);
    }
}
