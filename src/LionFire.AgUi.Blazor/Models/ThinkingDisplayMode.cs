namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Controls how thinking/reasoning content from AI models is displayed in chat messages.
/// </summary>
public enum ThinkingDisplayMode
{
    /// <summary>
    /// Thinking content is hidden and merged into the regular response (legacy behavior).
    /// </summary>
    Hide,

    /// <summary>
    /// Thinking content is shown in a collapsed panel that the user can expand.
    /// </summary>
    Collapsed,

    /// <summary>
    /// Thinking content is shown in a collapsed panel with a preview of the last few lines visible.
    /// </summary>
    CollapsedPreview,

    /// <summary>
    /// Thinking content is always shown expanded above the regular response.
    /// </summary>
    Expanded
}
