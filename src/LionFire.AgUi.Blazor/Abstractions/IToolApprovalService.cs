using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Service for managing tool call approvals, enabling human-in-the-loop workflows.
/// </summary>
/// <remarks>
/// <para>
/// This interface allows implementations to control how tool calls from AI agents are approved.
/// It supports various approval modes from fully automatic to requiring explicit user confirmation.
/// </para>
/// <para>
/// Tool approval is a critical security feature that prevents AI agents from executing
/// potentially harmful operations without user consent.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Requesting approval for a tool call
/// var toolCall = ToolCall.Create("delete_file", new Dictionary&lt;string, object&gt;
/// {
///     ["path"] = "/important/file.txt"
/// }, ToolRiskLevel.Dangerous);
///
/// var result = await approvalService.RequestApprovalAsync(toolCall);
/// if (result.IsApproved)
/// {
///     // Execute the tool call
/// }
/// else
/// {
///     // Log denial reason
///     Console.WriteLine($"Tool call denied: {result.DenialReason}");
/// }
/// </code>
/// </example>
public interface IToolApprovalService
{
    /// <summary>
    /// Requests approval for a tool call.
    /// </summary>
    /// <param name="toolCall">The tool call requiring approval.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ToolApprovalResult"/> indicating whether the call was approved.</returns>
    /// <remarks>
    /// <para>
    /// Depending on <see cref="ApprovalMode"/>, this method may:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="ToolApprovalMode.Blocking"/>: Block until user responds</description></item>
    /// <item><description><see cref="ToolApprovalMode.Async"/>: Return immediately with pending status</description></item>
    /// <item><description><see cref="ToolApprovalMode.AutoApprove"/>: Return immediately with approval</description></item>
    /// </list>
    /// </remarks>
    Task<ToolApprovalResult> RequestApprovalAsync(ToolCall toolCall, CancellationToken ct = default);

    /// <summary>
    /// Determines whether a tool call should be automatically approved based on configured policies.
    /// </summary>
    /// <param name="toolCall">The tool call to evaluate.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// <c>true</c> if the tool call can be auto-approved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method allows implementations to define policies for automatic approval,
    /// such as allowing all <see cref="ToolRiskLevel.Safe"/> operations or maintaining
    /// an allowlist of trusted tools.
    /// </remarks>
    Task<bool> ShouldApproveAutomaticallyAsync(ToolCall toolCall, CancellationToken ct = default);

    /// <summary>
    /// Gets the current approval mode.
    /// </summary>
    /// <value>The current <see cref="ToolApprovalMode"/>.</value>
    ToolApprovalMode ApprovalMode { get; }
}
