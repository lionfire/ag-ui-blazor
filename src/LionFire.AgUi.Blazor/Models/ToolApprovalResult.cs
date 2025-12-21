namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents the result of a tool approval request.
/// </summary>
/// <param name="IsApproved">Whether the tool call was approved.</param>
/// <param name="DenialReason">If denied, the reason for denial.</param>
/// <param name="ModifiedArguments">If approved, optionally modified arguments to use instead of the original.</param>
/// <param name="RespondedAt">The timestamp when the approval decision was made.</param>
public sealed record ToolApprovalResult(
    bool IsApproved,
    string? DenialReason,
    IReadOnlyDictionary<string, object>? ModifiedArguments,
    DateTimeOffset RespondedAt
)
{
    /// <summary>
    /// Creates an approved result.
    /// </summary>
    /// <param name="modifiedArguments">Optional modified arguments.</param>
    /// <returns>An approved <see cref="ToolApprovalResult"/>.</returns>
    public static ToolApprovalResult Approved(IReadOnlyDictionary<string, object>? modifiedArguments = null)
    {
        return new ToolApprovalResult(
            IsApproved: true,
            DenialReason: null,
            ModifiedArguments: modifiedArguments,
            RespondedAt: DateTimeOffset.UtcNow
        );
    }

    /// <summary>
    /// Creates a denied result.
    /// </summary>
    /// <param name="reason">The reason for denial.</param>
    /// <returns>A denied <see cref="ToolApprovalResult"/>.</returns>
    public static ToolApprovalResult Denied(string? reason = null)
    {
        return new ToolApprovalResult(
            IsApproved: false,
            DenialReason: reason,
            ModifiedArguments: null,
            RespondedAt: DateTimeOffset.UtcNow
        );
    }
}
