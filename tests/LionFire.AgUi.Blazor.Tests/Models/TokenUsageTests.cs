using LionFire.AgUi.Blazor.Models;
using FluentAssertions;
using System.Text.Json;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Tests for the TokenUsage record.
/// </summary>
public class TokenUsageTests
{
    [Fact]
    public void Create_ShouldCalculateTotalTokens()
    {
        var usage = TokenUsage.Create(100, 50);

        usage.PromptTokens.Should().Be(100);
        usage.CompletionTokens.Should().Be(50);
        usage.TotalTokens.Should().Be(150);
        usage.EstimatedCost.Should().BeNull();
        usage.ModelName.Should().BeNull();
    }

    [Fact]
    public void Create_WithModelName_ShouldSetModelName()
    {
        var usage = TokenUsage.Create(100, 50, "gpt-4");

        usage.ModelName.Should().Be("gpt-4");
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var usage = new TokenUsage(
            PromptTokens: 500,
            CompletionTokens: 200,
            TotalTokens: 700,
            EstimatedCost: 0.05m,
            ModelName: "claude-3-opus");

        usage.PromptTokens.Should().Be(500);
        usage.CompletionTokens.Should().Be(200);
        usage.TotalTokens.Should().Be(700);
        usage.EstimatedCost.Should().Be(0.05m);
        usage.ModelName.Should().Be("claude-3-opus");
    }

    [Fact]
    public void Add_ShouldCombineUsages()
    {
        var usage1 = TokenUsage.Create(100, 50, "model1");
        var usage2 = new TokenUsage(200, 100, 300, 0.02m, "model2");

        var combined = usage1.Add(usage2);

        combined.PromptTokens.Should().Be(300);
        combined.CompletionTokens.Should().Be(150);
        combined.TotalTokens.Should().Be(450);
        combined.EstimatedCost.Should().Be(0.02m); // 0 + 0.02
        combined.ModelName.Should().Be("model1"); // First non-null wins
    }

    [Fact]
    public void Add_WithNullCosts_ShouldHandleNulls()
    {
        var usage1 = TokenUsage.Create(100, 50);
        var usage2 = TokenUsage.Create(100, 50);

        var combined = usage1.Add(usage2);

        combined.EstimatedCost.Should().Be(0m);
    }

    [Fact]
    public void Add_ShouldPreserveFirstModelName()
    {
        var usage1 = TokenUsage.Create(100, 50);
        var usage2 = TokenUsage.Create(100, 50, "model2");

        var combined = usage1.Add(usage2);

        combined.ModelName.Should().Be("model2"); // Second has model, first is null
    }

    [Fact]
    public void JsonSerialization_ShouldRoundTrip()
    {
        var original = new TokenUsage(100, 50, 150, 0.01m, "test-model");

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<TokenUsage>(json);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void JsonSerialization_ShouldHandleNulls()
    {
        var original = TokenUsage.Create(100, 50);

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<TokenUsage>(json);

        deserialized.Should().NotBeNull();
        deserialized!.EstimatedCost.Should().BeNull();
        deserialized.ModelName.Should().BeNull();
    }

    [Fact]
    public void Equality_ShouldWorkForRecords()
    {
        var usage1 = new TokenUsage(100, 50, 150, 0.01m, "model");
        var usage2 = new TokenUsage(100, 50, 150, 0.01m, "model");

        usage1.Should().Be(usage2);
    }
}
