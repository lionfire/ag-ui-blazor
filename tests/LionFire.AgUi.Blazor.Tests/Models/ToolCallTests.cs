using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using System.Text.Json;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for the ToolCall record.
/// </summary>
public class ToolCallTests
{
    [Fact]
    public void Create_ShouldInitializeWithDefaults()
    {
        var toolCall = ToolCall.Create("read_file");

        toolCall.Id.Should().NotBeNullOrEmpty();
        toolCall.Name.Should().Be("read_file");
        toolCall.Description.Should().BeNull();
        toolCall.Arguments.Should().BeNull();
        toolCall.RiskLevel.Should().Be(ToolRiskLevel.Safe);
        toolCall.RequestedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithArguments_ShouldSetArguments()
    {
        var args = new Dictionary<string, object>
        {
            ["path"] = "/home/user/file.txt",
            ["encoding"] = "utf-8"
        };

        var toolCall = ToolCall.Create("read_file", args, ToolRiskLevel.Safe);

        toolCall.Arguments.Should().NotBeNull();
        toolCall.Arguments!["path"].Should().Be("/home/user/file.txt");
        toolCall.Arguments["encoding"].Should().Be("utf-8");
    }

    [Fact]
    public void Create_WithRiskLevel_ShouldSetRiskLevel()
    {
        var toolCall = ToolCall.Create("delete_file", riskLevel: ToolRiskLevel.Dangerous);

        toolCall.RiskLevel.Should().Be(ToolRiskLevel.Dangerous);
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var args = new Dictionary<string, object> { ["key"] = "value" };
        var requestedAt = DateTimeOffset.UtcNow;

        var toolCall = new ToolCall(
            Id: "tool-123",
            Name: "execute_command",
            Description: "Executes a shell command",
            Arguments: args,
            RiskLevel: ToolRiskLevel.Risky,
            RequestedAt: requestedAt);

        toolCall.Id.Should().Be("tool-123");
        toolCall.Name.Should().Be("execute_command");
        toolCall.Description.Should().Be("Executes a shell command");
        toolCall.Arguments.Should().ContainKey("key");
        toolCall.RiskLevel.Should().Be(ToolRiskLevel.Risky);
        toolCall.RequestedAt.Should().Be(requestedAt);
    }

    [Fact]
    public void JsonSerialization_ShouldRoundTrip()
    {
        var original = ToolCall.Create("test_tool",
            new Dictionary<string, object> { ["param"] = 123 },
            ToolRiskLevel.Risky);

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ToolCall>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be(original.Name);
        deserialized.RiskLevel.Should().Be(original.RiskLevel);
    }

    [Fact]
    public void Equality_ShouldWorkForRecords()
    {
        var now = DateTimeOffset.UtcNow;
        Dictionary<string, object>? args = null;

        var call1 = new ToolCall("id", "name", "desc", args, ToolRiskLevel.Safe, now);
        var call2 = new ToolCall("id", "name", "desc", args, ToolRiskLevel.Safe, now);

        call1.Should().Be(call2);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var call1 = ToolCall.Create("tool");
        var call2 = ToolCall.Create("tool");

        call1.Id.Should().NotBe(call2.Id);
    }
}
