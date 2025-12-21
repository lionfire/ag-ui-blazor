using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.Options;

namespace LionFire.AgUi.Blazor.Tests.Services;

public class TokenCostCalculatorTests
{
    private readonly TokenCostOptions _options;
    private readonly TokenCostCalculator _sut;

    public TokenCostCalculatorTests()
    {
        _options = new TokenCostOptions();
        _sut = new TokenCostCalculator(Options.Create(_options));
    }

    [Fact]
    public void CalculateCost_WithNull_ReturnsNull()
    {
        // Act
        var result = _sut.CalculateCost((TokenUsage?)null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CalculateCost_WithKnownModel_ReturnsCost()
    {
        // Arrange
        var usage = new TokenUsage(
            PromptTokens: 1000,
            CompletionTokens: 500,
            TotalTokens: 1500,
            ModelName: "gpt-4");

        // Act
        var result = _sut.CalculateCost(usage);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EstimatedCost);
        // gpt-4: $30/1M input, $60/1M output
        // 1000 * 30/1M + 500 * 60/1M = 0.03 + 0.03 = 0.06
        Assert.Equal(0.06m, result.EstimatedCost!.Value, precision: 4);
    }

    [Fact]
    public void CalculateCost_WithUnknownModel_NoDefaultRates_ReturnsNoChange()
    {
        // Arrange
        var usage = new TokenUsage(
            PromptTokens: 1000,
            CompletionTokens: 500,
            TotalTokens: 1500,
            ModelName: "unknown-model");

        // Act
        var result = _sut.CalculateCost(usage);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.EstimatedCost);
    }

    [Fact]
    public void CalculateCost_WithUnknownModel_WithDefaultRates_ReturnsDefaultCost()
    {
        // Arrange
        _options.DefaultRates = new ModelCostRates(1.0m, 2.0m);
        var usage = new TokenUsage(
            PromptTokens: 1000000, // 1M tokens
            CompletionTokens: 1000000, // 1M tokens
            TotalTokens: 2000000,
            ModelName: "unknown-model");

        // Act
        var result = _sut.CalculateCost(usage);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EstimatedCost);
        // 1M * 1.0/1M + 1M * 2.0/1M = 1 + 2 = 3
        Assert.Equal(3.0m, result.EstimatedCost!.Value, precision: 4);
    }

    [Fact]
    public void CalculateCost_IntOverload_WithKnownModel_ReturnsCost()
    {
        // Act
        var result = _sut.CalculateCost(1000, 500, "gpt-4");

        // Assert
        Assert.NotNull(result);
        // gpt-4: $30/1M input, $60/1M output
        Assert.Equal(0.06m, result!.Value, precision: 4);
    }

    [Fact]
    public void CalculateCost_IntOverload_WithUnknownModel_ReturnsNull()
    {
        // Act
        var result = _sut.CalculateCost(1000, 500, "unknown-model");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("gpt-4", 30.00, 60.00)]
    [InlineData("gpt-4o", 5.00, 15.00)]
    [InlineData("gpt-4o-mini", 0.15, 0.60)]
    [InlineData("claude-3-opus", 15.00, 75.00)]
    [InlineData("claude-3-haiku", 0.25, 1.25)]
    public void GetRates_KnownModels_ReturnsCorrectRates(string model, decimal expectedInput, decimal expectedOutput)
    {
        // Act
        var rates = _sut.GetRates(model);

        // Assert
        Assert.NotNull(rates);
        Assert.Equal(expectedInput, rates.InputCostPer1MTokens);
        Assert.Equal(expectedOutput, rates.OutputCostPer1MTokens);
    }

    [Fact]
    public void GetRates_PartialMatch_ReturnsMatchingRates()
    {
        // Arrange - "gpt-4-0314" should match "gpt-4"
        // Act
        var rates = _sut.GetRates("gpt-4-0314");

        // Assert
        Assert.NotNull(rates);
        Assert.Equal(30.00m, rates.InputCostPer1MTokens);
    }

    [Fact]
    public void GetRates_NullModelName_ReturnsDefaultRates()
    {
        // Arrange
        _options.DefaultRates = new ModelCostRates(1.0m, 2.0m);

        // Act
        var rates = _sut.GetRates(null);

        // Assert
        Assert.NotNull(rates);
        Assert.Equal(1.0m, rates.InputCostPer1MTokens);
    }

    [Fact]
    public void FormatCost_WithNull_ReturnsNA()
    {
        // Act
        var result = _sut.FormatCost(null);

        // Assert
        Assert.Equal("N/A", result);
    }

    [Fact]
    public void FormatCost_WithValue_ReturnsFormattedString()
    {
        // Act
        var result = _sut.FormatCost(0.1234m);

        // Assert
        Assert.Equal("$0.1234", result);
    }

    [Fact]
    public void FormatCost_WithCustomCurrency_UsesCustomSymbol()
    {
        // Arrange
        var options = new TokenCostOptions { CurrencySymbol = "€" };
        var calculator = new TokenCostCalculator(Options.Create(options));

        // Act
        var result = calculator.FormatCost(1.5m);

        // Assert
        Assert.StartsWith("€", result);
    }

    [Fact]
    public void CalculateCost_PreservesOriginalData()
    {
        // Arrange
        var usage = new TokenUsage(
            PromptTokens: 1000,
            CompletionTokens: 500,
            TotalTokens: 1500,
            ModelName: "gpt-4");

        // Act
        var result = _sut.CalculateCost(usage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.PromptTokens);
        Assert.Equal(500, result.CompletionTokens);
        Assert.Equal(1500, result.TotalTokens);
        Assert.Equal("gpt-4", result.ModelName);
    }

    [Fact]
    public void DefaultConstructor_CreatesWithDefaultOptions()
    {
        // Arrange & Act
        var calculator = new TokenCostCalculator();

        // Assert - Should work with default model rates
        var rates = calculator.GetRates("gpt-4");
        Assert.NotNull(rates);
    }
}
