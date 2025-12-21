using LionFire.AgUi.Blazor.Models;
using Microsoft.Extensions.AI;

namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Factory interface for obtaining agent instances and managing agent connections.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the creation and retrieval of AI agents, allowing different
/// implementations for Blazor Server (direct connection) and Blazor WASM (HTTP/SignalR proxy).
/// </para>
/// <para>
/// <strong>Blazor Server Implementation:</strong>
/// The server implementation typically creates agents directly using the underlying AI SDK,
/// maintaining connections within the server process.
/// </para>
/// <para>
/// <strong>Blazor WASM Implementation:</strong>
/// The WASM implementation proxies requests through HTTP or SignalR to a backend service
/// that manages the actual agent connections.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example usage in a Blazor component
/// @inject IAgentClientFactory AgentFactory
///
/// var agent = await AgentFactory.GetAgentAsync("assistant");
/// if (agent != null)
/// {
///     // Use the agent for chat
/// }
/// </code>
/// </example>
public interface IAgentClientFactory
{
    /// <summary>
    /// Gets an agent by name.
    /// </summary>
    /// <param name="agentName">The name of the agent to retrieve.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// The requested <see cref="IChatClient"/> if found and available; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Returns <c>null</c> if the agent is not found or not currently available.
    /// Use <see cref="ListAgentsAsync"/> to discover available agents.
    /// </remarks>
    Task<IChatClient?> GetAgentAsync(string agentName, CancellationToken ct = default);

    /// <summary>
    /// Lists all available agents.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="AgentInfo"/> describing available agents.</returns>
    /// <remarks>
    /// This method returns metadata about agents but does not establish connections.
    /// The <see cref="AgentInfo.IsAvailable"/> property indicates whether an agent
    /// can currently accept connections.
    /// </remarks>
    Task<IReadOnlyList<AgentInfo>> ListAgentsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the current connection state of the factory.
    /// </summary>
    /// <returns>The current <see cref="ConnectionState"/>.</returns>
    /// <remarks>
    /// For Blazor Server, this typically reflects the state of the underlying agent SDK connection.
    /// For Blazor WASM, this reflects the state of the connection to the backend proxy service.
    /// </remarks>
    ConnectionState GetConnectionState();
}
