using System.Collections.Concurrent;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// Configuration options for the tool approval service.
/// </summary>
public sealed class ToolApprovalOptions
{
    /// <summary>
    /// Gets or sets the default approval mode.
    /// Default is <see cref="ToolApprovalMode.Blocking"/>.
    /// </summary>
    public ToolApprovalMode DefaultMode { get; set; } = ToolApprovalMode.Blocking;

    /// <summary>
    /// Gets or sets the timeout for blocking approval requests.
    /// Default is 5 minutes.
    /// </summary>
    public TimeSpan ApprovalTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether safe tool calls should be auto-approved.
    /// Default is false.
    /// </summary>
    public bool AutoApproveSafe { get; set; } = false;

    /// <summary>
    /// Gets or sets a list of tool names that are always auto-approved.
    /// </summary>
    public HashSet<string> AllowlistedTools { get; set; } = new();

    /// <summary>
    /// Gets or sets a list of tool names that always require manual approval.
    /// </summary>
    public HashSet<string> BlocklistedTools { get; set; } = new();
}

/// <summary>
/// Service for managing tool call approvals with blocking modal support.
/// </summary>
/// <remarks>
/// <para>
/// This service maintains pending approval requests and notifies UI components
/// when new approvals are needed. In blocking mode, the RequestApprovalAsync
/// method blocks until the user approves or denies the request.
/// </para>
/// </remarks>
public class ToolApprovalService : IToolApprovalService
{
    private readonly IOptions<ToolApprovalOptions> _options;
    private readonly ILogger<ToolApprovalService> _logger;
    private readonly ConcurrentDictionary<string, PendingApproval> _pendingApprovals = new();
    private readonly ConcurrentBag<ToolApprovalAuditEntry> _auditTrail = new();

    /// <summary>
    /// Occurs when a new tool approval is requested.
    /// </summary>
    public event EventHandler<ToolCall>? ApprovalRequested;

    /// <summary>
    /// Occurs when a tool approval request is resolved (approved or denied).
    /// </summary>
    public event EventHandler<ToolApprovalAuditEntry>? ApprovalResolved;

    /// <inheritdoc />
    public ToolApprovalMode ApprovalMode => _options.Value.DefaultMode;

    /// <summary>
    /// Initializes a new instance of <see cref="ToolApprovalService"/>.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public ToolApprovalService(
        IOptions<ToolApprovalOptions> options,
        ILogger<ToolApprovalService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ToolApprovalResult> RequestApprovalAsync(ToolCall toolCall, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(toolCall);

        _logger.LogInformation(
            "Tool approval requested: {ToolName} (Risk: {RiskLevel})",
            toolCall.Name,
            toolCall.RiskLevel);

        // Check for automatic approval
        if (await ShouldApproveAutomaticallyAsync(toolCall, ct))
        {
            _logger.LogDebug("Auto-approving tool call: {ToolName}", toolCall.Name);
            var autoResult = ToolApprovalResult.Approved();
            RecordAuditEntry(toolCall, autoResult, wasAutomatic: true);
            return autoResult;
        }

        // Check if tool is blocklisted
        if (_options.Value.BlocklistedTools.Contains(toolCall.Name))
        {
            _logger.LogWarning("Tool call denied (blocklisted): {ToolName}", toolCall.Name);
            var deniedResult = ToolApprovalResult.Denied("Tool is on the blocklist");
            RecordAuditEntry(toolCall, deniedResult, wasAutomatic: true);
            return deniedResult;
        }

        switch (_options.Value.DefaultMode)
        {
            case ToolApprovalMode.AutoApprove:
                var approveResult = ToolApprovalResult.Approved();
                RecordAuditEntry(toolCall, approveResult, wasAutomatic: true);
                return approveResult;

            case ToolApprovalMode.Blocking:
                return await RequestBlockingApprovalAsync(toolCall, ct);

            case ToolApprovalMode.Async:
                // Queue for later approval, return pending
                QueuePendingApproval(toolCall);
                // For async mode, we still need to wait but with a longer timeout
                return await RequestBlockingApprovalAsync(toolCall, ct);

            default:
                throw new InvalidOperationException($"Unknown approval mode: {_options.Value.DefaultMode}");
        }
    }

    /// <inheritdoc />
    public Task<bool> ShouldApproveAutomaticallyAsync(ToolCall toolCall, CancellationToken ct = default)
    {
        var opts = _options.Value;

        // Check allowlist first
        if (opts.AllowlistedTools.Contains(toolCall.Name))
        {
            return Task.FromResult(true);
        }

        // Auto-approve safe operations if enabled
        if (opts.AutoApproveSafe && toolCall.RiskLevel == ToolRiskLevel.Safe)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Gets all pending approval requests.
    /// </summary>
    /// <returns>A read-only list of pending tool calls.</returns>
    public IReadOnlyList<ToolCall> GetPendingApprovals()
    {
        return _pendingApprovals.Values
            .Select(p => p.ToolCall)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Approves a pending tool call.
    /// </summary>
    /// <param name="toolCallId">The ID of the tool call to approve.</param>
    /// <param name="modifiedArguments">Optional modified arguments.</param>
    public void Approve(string toolCallId, IReadOnlyDictionary<string, object>? modifiedArguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolCallId);

        if (_pendingApprovals.TryGetValue(toolCallId, out var pending))
        {
            var result = ToolApprovalResult.Approved(modifiedArguments);
            pending.SetResult(result);
            RecordAuditEntry(pending.ToolCall, result, wasAutomatic: false);

            _logger.LogInformation("Tool call approved: {ToolName}", pending.ToolCall.Name);
        }
        else
        {
            _logger.LogWarning("Attempted to approve non-existent tool call: {ToolCallId}", toolCallId);
        }
    }

    /// <summary>
    /// Denies a pending tool call.
    /// </summary>
    /// <param name="toolCallId">The ID of the tool call to deny.</param>
    /// <param name="reason">The reason for denial.</param>
    public void Deny(string toolCallId, string? reason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolCallId);

        if (_pendingApprovals.TryGetValue(toolCallId, out var pending))
        {
            var result = ToolApprovalResult.Denied(reason);
            pending.SetResult(result);
            RecordAuditEntry(pending.ToolCall, result, wasAutomatic: false);

            _logger.LogInformation("Tool call denied: {ToolName} - {Reason}", pending.ToolCall.Name, reason);
        }
        else
        {
            _logger.LogWarning("Attempted to deny non-existent tool call: {ToolCallId}", toolCallId);
        }
    }

    /// <summary>
    /// Gets the audit trail of approval decisions.
    /// </summary>
    /// <param name="limit">Maximum number of entries to return.</param>
    /// <returns>A read-only list of audit entries.</returns>
    public IReadOnlyList<ToolApprovalAuditEntry> GetAuditTrail(int limit = 100)
    {
        return _auditTrail
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList()
            .AsReadOnly();
    }

    private async Task<ToolApprovalResult> RequestBlockingApprovalAsync(ToolCall toolCall, CancellationToken ct)
    {
        var pending = new PendingApproval(toolCall);
        _pendingApprovals[toolCall.Id] = pending;

        ApprovalRequested?.Invoke(this, toolCall);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(_options.Value.ApprovalTimeout);

            return await pending.WaitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            var result = ToolApprovalResult.Denied("Approval request timed out");
            RecordAuditEntry(toolCall, result, wasAutomatic: true);
            _logger.LogWarning("Tool approval timed out: {ToolName}", toolCall.Name);
            return result;
        }
        finally
        {
            _pendingApprovals.TryRemove(toolCall.Id, out _);
        }
    }

    private void QueuePendingApproval(ToolCall toolCall)
    {
        var pending = new PendingApproval(toolCall);
        _pendingApprovals[toolCall.Id] = pending;
        ApprovalRequested?.Invoke(this, toolCall);
    }

    private void RecordAuditEntry(ToolCall toolCall, ToolApprovalResult result, bool wasAutomatic)
    {
        var entry = new ToolApprovalAuditEntry(
            toolCall,
            result,
            wasAutomatic,
            DateTimeOffset.UtcNow);

        _auditTrail.Add(entry);
        ApprovalResolved?.Invoke(this, entry);
    }

    private sealed class PendingApproval
    {
        private readonly TaskCompletionSource<ToolApprovalResult> _tcs = new();

        public ToolCall ToolCall { get; }

        public PendingApproval(ToolCall toolCall)
        {
            ToolCall = toolCall;
        }

        public void SetResult(ToolApprovalResult result)
        {
            _tcs.TrySetResult(result);
        }

        public Task<ToolApprovalResult> WaitAsync(CancellationToken ct)
        {
            ct.Register(() => _tcs.TrySetCanceled(ct));
            return _tcs.Task;
        }
    }
}

/// <summary>
/// Audit trail entry for a tool approval decision.
/// </summary>
public sealed record ToolApprovalAuditEntry(
    ToolCall ToolCall,
    ToolApprovalResult Result,
    bool WasAutomatic,
    DateTimeOffset Timestamp
);
