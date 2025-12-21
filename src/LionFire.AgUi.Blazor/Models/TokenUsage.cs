namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents token usage statistics for a conversation or message.
/// </summary>
/// <param name="PromptTokens">The number of tokens in the prompt/input.</param>
/// <param name="CompletionTokens">The number of tokens in the completion/output.</param>
/// <param name="TotalTokens">The total number of tokens used.</param>
/// <param name="EstimatedCost">The estimated cost in USD, if available.</param>
/// <param name="ModelName">The name of the model used, if available.</param>
public sealed record TokenUsage(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    decimal? EstimatedCost = null,
    string? ModelName = null
)
{
    /// <summary>
    /// Creates a token usage record with automatic total calculation.
    /// </summary>
    /// <param name="promptTokens">The number of prompt tokens.</param>
    /// <param name="completionTokens">The number of completion tokens.</param>
    /// <param name="modelName">Optional model name.</param>
    /// <returns>A new <see cref="TokenUsage"/> instance.</returns>
    public static TokenUsage Create(int promptTokens, int completionTokens, string? modelName = null)
    {
        return new TokenUsage(
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens,
            TotalTokens: promptTokens + completionTokens,
            EstimatedCost: null,
            ModelName: modelName
        );
    }

    /// <summary>
    /// Combines two token usage records.
    /// </summary>
    /// <param name="other">The other token usage to add.</param>
    /// <returns>A combined <see cref="TokenUsage"/> instance.</returns>
    public TokenUsage Add(TokenUsage other)
    {
        return new TokenUsage(
            PromptTokens: PromptTokens + other.PromptTokens,
            CompletionTokens: CompletionTokens + other.CompletionTokens,
            TotalTokens: TotalTokens + other.TotalTokens,
            EstimatedCost: (EstimatedCost ?? 0) + (other.EstimatedCost ?? 0),
            ModelName: ModelName ?? other.ModelName
        );
    }
}
