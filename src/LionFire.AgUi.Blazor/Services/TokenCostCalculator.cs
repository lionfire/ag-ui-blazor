using LionFire.AgUi.Blazor.Models;
using Microsoft.Extensions.Options;

namespace LionFire.AgUi.Blazor.Services;

/// <summary>
/// Configuration options for token cost calculation.
/// </summary>
public sealed class TokenCostOptions
{
    /// <summary>
    /// Gets or sets the cost rates per model.
    /// Key is the model name (case-insensitive), value contains the rates.
    /// </summary>
    public Dictionary<string, ModelCostRates> ModelRates { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        // Default rates for common models (USD per 1M tokens)
        ["gpt-4"] = new ModelCostRates(30.00m, 60.00m),
        ["gpt-4-turbo"] = new ModelCostRates(10.00m, 30.00m),
        ["gpt-4o"] = new ModelCostRates(5.00m, 15.00m),
        ["gpt-4o-mini"] = new ModelCostRates(0.15m, 0.60m),
        ["gpt-3.5-turbo"] = new ModelCostRates(0.50m, 1.50m),
        ["claude-3-opus"] = new ModelCostRates(15.00m, 75.00m),
        ["claude-3-sonnet"] = new ModelCostRates(3.00m, 15.00m),
        ["claude-3-haiku"] = new ModelCostRates(0.25m, 1.25m),
        ["claude-3.5-sonnet"] = new ModelCostRates(3.00m, 15.00m)
    };

    /// <summary>
    /// Gets or sets the default rates to use when a model is not found.
    /// Null means no cost will be calculated for unknown models.
    /// </summary>
    public ModelCostRates? DefaultRates { get; set; }

    /// <summary>
    /// Gets or sets the currency symbol for display.
    /// Default is "$".
    /// </summary>
    public string CurrencySymbol { get; set; } = "$";

    /// <summary>
    /// Gets or sets the number of decimal places for cost display.
    /// Default is 4.
    /// </summary>
    public int CostDecimalPlaces { get; set; } = 4;
}

/// <summary>
/// Cost rates for a specific model.
/// </summary>
/// <param name="InputCostPer1MTokens">Cost in USD per 1 million input/prompt tokens.</param>
/// <param name="OutputCostPer1MTokens">Cost in USD per 1 million output/completion tokens.</param>
public sealed record ModelCostRates(
    decimal InputCostPer1MTokens,
    decimal OutputCostPer1MTokens
);

/// <summary>
/// Service for calculating token costs based on model pricing.
/// </summary>
public interface ITokenCostCalculator
{
    /// <summary>
    /// Calculates the estimated cost for a given token usage.
    /// </summary>
    /// <param name="tokenUsage">The token usage to calculate cost for.</param>
    /// <returns>The token usage with estimated cost populated, or null if no cost could be calculated.</returns>
    TokenUsage? CalculateCost(TokenUsage? tokenUsage);

    /// <summary>
    /// Calculates the cost for given token counts and model.
    /// </summary>
    /// <param name="promptTokens">Number of prompt/input tokens.</param>
    /// <param name="completionTokens">Number of completion/output tokens.</param>
    /// <param name="modelName">The model name.</param>
    /// <returns>The estimated cost in USD, or null if no rate is configured for the model.</returns>
    decimal? CalculateCost(int promptTokens, int completionTokens, string? modelName);

    /// <summary>
    /// Gets the cost rates for a specific model.
    /// </summary>
    /// <param name="modelName">The model name.</param>
    /// <returns>The cost rates, or null if not configured.</returns>
    ModelCostRates? GetRates(string? modelName);

    /// <summary>
    /// Formats a cost value for display.
    /// </summary>
    /// <param name="cost">The cost to format.</param>
    /// <returns>A formatted string representing the cost.</returns>
    string FormatCost(decimal? cost);
}

/// <summary>
/// Default implementation of <see cref="ITokenCostCalculator"/>.
/// </summary>
public sealed class TokenCostCalculator : ITokenCostCalculator
{
    private readonly TokenCostOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="TokenCostCalculator"/>.
    /// </summary>
    public TokenCostCalculator(IOptions<TokenCostOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Initializes a new instance with default options.
    /// </summary>
    public TokenCostCalculator()
        : this(Options.Create(new TokenCostOptions()))
    {
    }

    /// <inheritdoc />
    public TokenUsage? CalculateCost(TokenUsage? tokenUsage)
    {
        if (tokenUsage == null)
            return null;

        var cost = CalculateCost(
            tokenUsage.PromptTokens,
            tokenUsage.CompletionTokens,
            tokenUsage.ModelName);

        if (cost == null)
            return tokenUsage;

        return tokenUsage with { EstimatedCost = cost };
    }

    /// <inheritdoc />
    public decimal? CalculateCost(int promptTokens, int completionTokens, string? modelName)
    {
        var rates = GetRates(modelName);
        if (rates == null)
            return null;

        var inputCost = (promptTokens / 1_000_000m) * rates.InputCostPer1MTokens;
        var outputCost = (completionTokens / 1_000_000m) * rates.OutputCostPer1MTokens;

        return Math.Round(inputCost + outputCost, _options.CostDecimalPlaces);
    }

    /// <inheritdoc />
    public ModelCostRates? GetRates(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return _options.DefaultRates;

        // Try exact match first
        if (_options.ModelRates.TryGetValue(modelName, out var rates))
            return rates;

        // Try partial match (model name might include version/variant)
        foreach (var kvp in _options.ModelRates)
        {
            if (modelName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return _options.DefaultRates;
    }

    /// <inheritdoc />
    public string FormatCost(decimal? cost)
    {
        if (cost == null)
            return "N/A";

        return $"{_options.CurrencySymbol}{cost:N4}";
    }
}
