namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents a tool call request from an agent that may require user approval.
/// </summary>
/// <param name="Id">The unique identifier for this tool call.</param>
/// <param name="Name">The name of the tool being invoked.</param>
/// <param name="Description">An optional human-readable description of what the tool does.</param>
/// <param name="Arguments">The arguments to be passed to the tool.</param>
/// <param name="RiskLevel">The assessed risk level of this tool call.</param>
/// <param name="RequestedAt">The timestamp when the tool call was requested.</param>
public sealed record ToolCall(
    string Id,
    string Name,
    string? Description,
    IReadOnlyDictionary<string, object>? Arguments,
    ToolRiskLevel RiskLevel,
    DateTimeOffset RequestedAt
)
{
    /// <summary>
    /// Creates a new tool call with default values.
    /// </summary>
    /// <param name="name">The name of the tool.</param>
    /// <param name="arguments">Optional arguments for the tool.</param>
    /// <param name="riskLevel">The risk level, defaults to <see cref="ToolRiskLevel.Safe"/>.</param>
    /// <returns>A new <see cref="ToolCall"/> instance.</returns>
    public static ToolCall Create(
        string name,
        IReadOnlyDictionary<string, object>? arguments = null,
        ToolRiskLevel riskLevel = ToolRiskLevel.Safe)
    {
        return new ToolCall(
            Id: Guid.NewGuid().ToString(),
            Name: name,
            Description: null,
            Arguments: arguments,
            RiskLevel: riskLevel,
            RequestedAt: DateTimeOffset.UtcNow
        );
    }
}
