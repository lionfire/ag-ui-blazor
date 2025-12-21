using LionFire.AgUi.Blazor.Models;
using FluentAssertions;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for enum definitions to ensure all expected values exist.
/// </summary>
public class EnumTests
{
    [Fact]
    public void ConnectionState_ShouldHaveAllExpectedValues()
    {
        // Verify all enum values exist and are distinct
        var values = Enum.GetValues<ConnectionState>();
        values.Should().HaveCount(5);
        values.Should().Contain(ConnectionState.Connected);
        values.Should().Contain(ConnectionState.Connecting);
        values.Should().Contain(ConnectionState.Disconnected);
        values.Should().Contain(ConnectionState.Reconnecting);
        values.Should().Contain(ConnectionState.Error);
    }

    [Fact]
    public void ToolApprovalMode_ShouldHaveAllExpectedValues()
    {
        var values = Enum.GetValues<ToolApprovalMode>();
        values.Should().HaveCount(3);
        values.Should().Contain(ToolApprovalMode.Blocking);
        values.Should().Contain(ToolApprovalMode.Async);
        values.Should().Contain(ToolApprovalMode.AutoApprove);
    }

    [Fact]
    public void ToolRiskLevel_ShouldHaveAllExpectedValues()
    {
        var values = Enum.GetValues<ToolRiskLevel>();
        values.Should().HaveCount(3);
        values.Should().Contain(ToolRiskLevel.Safe);
        values.Should().Contain(ToolRiskLevel.Risky);
        values.Should().Contain(ToolRiskLevel.Dangerous);
    }

    [Theory]
    [InlineData(ConnectionState.Connected, 0)]
    [InlineData(ConnectionState.Connecting, 1)]
    [InlineData(ConnectionState.Disconnected, 2)]
    [InlineData(ConnectionState.Reconnecting, 3)]
    [InlineData(ConnectionState.Error, 4)]
    public void ConnectionState_ShouldHaveExpectedIntValues(ConnectionState state, int expectedValue)
    {
        ((int)state).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ToolRiskLevel.Safe, 0)]
    [InlineData(ToolRiskLevel.Risky, 1)]
    [InlineData(ToolRiskLevel.Dangerous, 2)]
    public void ToolRiskLevel_ShouldHaveExpectedIntValues(ToolRiskLevel level, int expectedValue)
    {
        ((int)level).Should().Be(expectedValue);
    }
}
