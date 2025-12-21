namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Specifies how tool approval requests are handled.
/// </summary>
public enum ToolApprovalMode
{
    /// <summary>
    /// Tool execution blocks until explicit user approval is received.
    /// The agent will wait indefinitely for a response.
    /// </summary>
    Blocking,

    /// <summary>
    /// Tool approval is requested asynchronously, allowing the user to approve
    /// or deny at their convenience. The agent may continue with other tasks.
    /// </summary>
    Async,

    /// <summary>
    /// All tool calls are automatically approved without user intervention.
    /// Use with caution, as this bypasses security checks.
    /// </summary>
    AutoApprove
}
