using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using System.Text.Json;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for the ToolApprovalResult record.
/// </summary>
public class ToolApprovalResultTests
{
    [Fact]
    public void Approved_ShouldCreateApprovedResult()
    {
        var result = ToolApprovalResult.Approved();

        result.IsApproved.Should().BeTrue();
        result.DenialReason.Should().BeNull();
        result.ModifiedArguments.Should().BeNull();
        result.RespondedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Approved_WithModifiedArguments_ShouldIncludeArguments()
    {
        var modifiedArgs = new Dictionary<string, object>
        {
            ["path"] = "/safe/path.txt"
        };

        var result = ToolApprovalResult.Approved(modifiedArgs);

        result.IsApproved.Should().BeTrue();
        result.ModifiedArguments.Should().NotBeNull();
        result.ModifiedArguments!["path"].Should().Be("/safe/path.txt");
    }

    [Fact]
    public void Denied_ShouldCreateDeniedResult()
    {
        var result = ToolApprovalResult.Denied();

        result.IsApproved.Should().BeFalse();
        result.DenialReason.Should().BeNull();
        result.ModifiedArguments.Should().BeNull();
        result.RespondedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Denied_WithReason_ShouldIncludeReason()
    {
        var result = ToolApprovalResult.Denied("User declined the operation");

        result.IsApproved.Should().BeFalse();
        result.DenialReason.Should().Be("User declined the operation");
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var modifiedArgs = new Dictionary<string, object> { ["modified"] = true };
        var respondedAt = DateTimeOffset.UtcNow;

        var result = new ToolApprovalResult(
            IsApproved: true,
            DenialReason: null,
            ModifiedArguments: modifiedArgs,
            RespondedAt: respondedAt);

        result.IsApproved.Should().BeTrue();
        result.DenialReason.Should().BeNull();
        result.ModifiedArguments.Should().ContainKey("modified");
        result.RespondedAt.Should().Be(respondedAt);
    }

    [Fact]
    public void JsonSerialization_ShouldRoundTrip_Approved()
    {
        var original = ToolApprovalResult.Approved();

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ToolApprovalResult>(json);

        deserialized.Should().NotBeNull();
        deserialized!.IsApproved.Should().Be(original.IsApproved);
    }

    [Fact]
    public void JsonSerialization_ShouldRoundTrip_Denied()
    {
        var original = ToolApprovalResult.Denied("Not allowed");

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ToolApprovalResult>(json);

        deserialized.Should().NotBeNull();
        deserialized!.IsApproved.Should().BeFalse();
        deserialized.DenialReason.Should().Be("Not allowed");
    }

    [Fact]
    public void Equality_ShouldWorkForRecords()
    {
        var now = DateTimeOffset.UtcNow;

        var result1 = new ToolApprovalResult(true, null, null, now);
        var result2 = new ToolApprovalResult(true, null, null, now);

        result1.Should().Be(result2);
    }
}
