using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using System.Text.Json;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for the AgentInfo record.
/// </summary>
public class AgentInfoTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var info = new AgentInfo(
            Name: "test-agent",
            Description: "A test agent",
            IconUrl: "https://example.com/icon.png",
            IsAvailable: true);

        info.Name.Should().Be("test-agent");
        info.Description.Should().Be("A test agent");
        info.IconUrl.Should().Be("https://example.com/icon.png");
        info.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithOptionalDefaults_ShouldUseDefaults()
    {
        var info = new AgentInfo(Name: "minimal-agent");

        info.Name.Should().Be("minimal-agent");
        info.Description.Should().BeNull();
        info.IconUrl.Should().BeNull();
        info.IsAvailable.Should().BeTrue(); // Default is true
    }

    [Fact]
    public void Equality_ShouldWorkForRecords()
    {
        var info1 = new AgentInfo("agent", "desc", "icon", true);
        var info2 = new AgentInfo("agent", "desc", "icon", true);

        info1.Should().Be(info2);
        (info1 == info2).Should().BeTrue();
    }

    [Fact]
    public void Inequality_ShouldWorkForDifferentValues()
    {
        var info1 = new AgentInfo("agent1");
        var info2 = new AgentInfo("agent2");

        info1.Should().NotBe(info2);
        (info1 != info2).Should().BeTrue();
    }

    [Fact]
    public void WithExpression_ShouldCreateModifiedCopy()
    {
        var original = new AgentInfo("agent", "desc", "icon", true);
        var modified = original with { IsAvailable = false };

        original.IsAvailable.Should().BeTrue();
        modified.IsAvailable.Should().BeFalse();
        modified.Name.Should().Be(original.Name);
    }

    [Fact]
    public void JsonSerialization_ShouldRoundTrip()
    {
        var original = new AgentInfo(
            Name: "json-agent",
            Description: "For JSON testing",
            IconUrl: "https://example.com/icon.png",
            IsAvailable: false);

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<AgentInfo>(json);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void JsonDeserialization_ShouldHandleNulls()
    {
        var json = """{"Name":"null-test"}""";
        var deserialized = JsonSerializer.Deserialize<AgentInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("null-test");
        deserialized.Description.Should().BeNull();
    }
}
