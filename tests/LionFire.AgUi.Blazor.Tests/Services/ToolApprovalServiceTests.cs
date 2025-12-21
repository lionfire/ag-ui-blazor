using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LionFire.AgUi.Blazor.Tests.Services;

public class ToolApprovalServiceTests
{
    private readonly ToolApprovalOptions _options;
    private readonly ToolApprovalService _sut;

    public ToolApprovalServiceTests()
    {
        _options = new ToolApprovalOptions();
        _sut = new ToolApprovalService(
            Options.Create(_options),
            NullLogger<ToolApprovalService>.Instance);
    }

    [Fact]
    public void ApprovalMode_ReturnsDefaultMode()
    {
        // Assert
        Assert.Equal(ToolApprovalMode.Blocking, _sut.ApprovalMode);
    }

    [Fact]
    public void GetPendingApprovals_ReturnsEmptyInitially()
    {
        // Act
        var pending = _sut.GetPendingApprovals();

        // Assert
        Assert.Empty(pending);
    }

    [Fact]
    public async Task ShouldApproveAutomatically_ReturnsFalseByDefault()
    {
        // Arrange
        var toolCall = CreateToolCall("test-tool", ToolRiskLevel.Safe);

        // Act
        var result = await _sut.ShouldApproveAutomaticallyAsync(toolCall);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldApproveAutomatically_ReturnsTrueForAllowlistedTool()
    {
        // Arrange
        _options.AllowlistedTools.Add("safe-tool");
        var toolCall = CreateToolCall("safe-tool", ToolRiskLevel.Safe);

        // Act
        var result = await _sut.ShouldApproveAutomaticallyAsync(toolCall);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldApproveAutomatically_ReturnsTrueWhenAutoApproveSafeEnabled()
    {
        // Arrange
        _options.AutoApproveSafe = true;
        var toolCall = CreateToolCall("any-tool", ToolRiskLevel.Safe);

        // Act
        var result = await _sut.ShouldApproveAutomaticallyAsync(toolCall);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldApproveAutomatically_ReturnsFalseForRiskyWhenAutoApproveSafeEnabled()
    {
        // Arrange
        _options.AutoApproveSafe = true;
        var toolCall = CreateToolCall("risky-tool", ToolRiskLevel.Risky);

        // Act
        var result = await _sut.ShouldApproveAutomaticallyAsync(toolCall);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RequestApprovalAsync_ThrowsOnNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.RequestApprovalAsync(null!));
    }

    [Fact]
    public async Task RequestApprovalAsync_AutoApprovesAllowlistedTool()
    {
        // Arrange
        _options.AllowlistedTools.Add("auto-tool");
        var toolCall = CreateToolCall("auto-tool", ToolRiskLevel.Safe);

        // Act
        var result = await _sut.RequestApprovalAsync(toolCall);

        // Assert
        Assert.True(result.IsApproved);
    }

    [Fact]
    public async Task RequestApprovalAsync_DeniesBlocklistedTool()
    {
        // Arrange
        _options.BlocklistedTools.Add("blocked-tool");
        var toolCall = CreateToolCall("blocked-tool", ToolRiskLevel.Safe);

        // Act
        var result = await _sut.RequestApprovalAsync(toolCall);

        // Assert
        Assert.False(result.IsApproved);
        Assert.Equal("Tool is on the blocklist", result.DenialReason);
    }

    [Fact]
    public async Task RequestApprovalAsync_AutoApprovesModeAutoApprove()
    {
        // Arrange
        _options.DefaultMode = ToolApprovalMode.AutoApprove;
        var toolCall = CreateToolCall("any-tool", ToolRiskLevel.Dangerous);

        // Act
        var result = await _sut.RequestApprovalAsync(toolCall);

        // Assert
        Assert.True(result.IsApproved);
    }

    [Fact]
    public async Task Approve_ApprovesToolCall()
    {
        // Arrange
        var toolCall = CreateToolCall("blocking-tool", ToolRiskLevel.Risky);
        var approvalTask = _sut.RequestApprovalAsync(toolCall);

        // Give time for the request to be queued
        await Task.Delay(50);

        // Act
        _sut.Approve(toolCall.Id);
        var result = await approvalTask;

        // Assert
        Assert.True(result.IsApproved);
    }

    [Fact]
    public async Task Deny_DeniesToolCall()
    {
        // Arrange
        var toolCall = CreateToolCall("blocking-tool", ToolRiskLevel.Risky);
        var approvalTask = _sut.RequestApprovalAsync(toolCall);

        // Give time for the request to be queued
        await Task.Delay(50);

        // Act
        _sut.Deny(toolCall.Id, "Test denial reason");
        var result = await approvalTask;

        // Assert
        Assert.False(result.IsApproved);
        Assert.Equal("Test denial reason", result.DenialReason);
    }

    [Fact]
    public void Approve_ThrowsOnNullOrWhitespace()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Approve(null!));
        Assert.Throws<ArgumentException>(() => _sut.Approve(""));
        Assert.Throws<ArgumentException>(() => _sut.Approve("   "));
    }

    [Fact]
    public void Deny_ThrowsOnNullOrWhitespace()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Deny(null!));
        Assert.Throws<ArgumentException>(() => _sut.Deny(""));
        Assert.Throws<ArgumentException>(() => _sut.Deny("   "));
    }

    [Fact]
    public async Task ApprovalRequested_RaisesEvent()
    {
        // Arrange
        var eventRaised = false;
        ToolCall? receivedToolCall = null;
        _sut.ApprovalRequested += (sender, tc) =>
        {
            eventRaised = true;
            receivedToolCall = tc;
        };
        var toolCall = CreateToolCall("event-tool", ToolRiskLevel.Risky);

        // Act
        var approvalTask = _sut.RequestApprovalAsync(toolCall);
        await Task.Delay(50);
        _sut.Approve(toolCall.Id);
        await approvalTask;

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(toolCall.Id, receivedToolCall?.Id);
    }

    [Fact]
    public async Task ApprovalResolved_RaisesEvent()
    {
        // Arrange
        var eventRaised = false;
        ToolApprovalAuditEntry? receivedEntry = null;
        _sut.ApprovalResolved += (sender, entry) =>
        {
            eventRaised = true;
            receivedEntry = entry;
        };
        var toolCall = CreateToolCall("event-tool", ToolRiskLevel.Risky);

        // Act
        var approvalTask = _sut.RequestApprovalAsync(toolCall);
        await Task.Delay(50);
        _sut.Approve(toolCall.Id);
        await approvalTask;

        // Assert
        Assert.True(eventRaised);
        Assert.NotNull(receivedEntry);
        Assert.Equal(toolCall.Id, receivedEntry.ToolCall.Id);
        Assert.True(receivedEntry.Result.IsApproved);
    }

    [Fact]
    public async Task GetAuditTrail_ReturnsEntries()
    {
        // Arrange
        _options.AllowlistedTools.Add("audit-tool");
        var toolCall = CreateToolCall("audit-tool", ToolRiskLevel.Safe);

        // Act
        await _sut.RequestApprovalAsync(toolCall);
        var auditTrail = _sut.GetAuditTrail();

        // Assert
        Assert.Single(auditTrail);
        Assert.Equal(toolCall.Id, auditTrail[0].ToolCall.Id);
        Assert.True(auditTrail[0].WasAutomatic);
    }

    [Fact]
    public async Task RequestApprovalAsync_TimesOut()
    {
        // Arrange
        _options.ApprovalTimeout = TimeSpan.FromMilliseconds(100);
        var toolCall = CreateToolCall("timeout-tool", ToolRiskLevel.Risky);

        // Act
        var result = await _sut.RequestApprovalAsync(toolCall);

        // Assert
        Assert.False(result.IsApproved);
        Assert.Equal("Approval request timed out", result.DenialReason);
    }

    [Fact]
    public async Task Approve_WithModifiedArguments()
    {
        // Arrange
        var toolCall = CreateToolCall("modify-tool", ToolRiskLevel.Safe);
        var approvalTask = _sut.RequestApprovalAsync(toolCall);
        await Task.Delay(50);

        var modifiedArgs = new Dictionary<string, object> { { "modified", true } };

        // Act
        _sut.Approve(toolCall.Id, modifiedArgs);
        var result = await approvalTask;

        // Assert
        Assert.True(result.IsApproved);
        Assert.NotNull(result.ModifiedArguments);
        Assert.True((bool)result.ModifiedArguments["modified"]);
    }

    private static ToolCall CreateToolCall(string name, ToolRiskLevel riskLevel)
    {
        return new ToolCall(
            Id: Guid.NewGuid().ToString(),
            Name: name,
            Description: $"Test tool: {name}",
            Arguments: new Dictionary<string, object> { { "arg1", "value1" } },
            RiskLevel: riskLevel,
            RequestedAt: DateTimeOffset.UtcNow);
    }
}
