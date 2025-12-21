namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Provides metadata about an available agent.
/// </summary>
/// <param name="Name">The unique identifier/name of the agent.</param>
/// <param name="Description">An optional human-readable description of the agent's purpose and capabilities.</param>
/// <param name="IconUrl">An optional URL to an icon representing the agent.</param>
/// <param name="IsAvailable">Indicates whether the agent is currently available for use.</param>
public sealed record AgentInfo(
    string Name,
    string? Description = null,
    string? IconUrl = null,
    bool IsAvailable = true
);
