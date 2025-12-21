namespace BlazorWasm.Full.Shared;

/// <summary>
/// Represents information about an available AI agent.
/// </summary>
public record AgentInfo(string Name, string DisplayName, string? Description = null);
