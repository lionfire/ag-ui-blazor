using Microsoft.AspNetCore.Components;

namespace LionFire.AgUi.Blazor.MudBlazor.Components;

/// <summary>
/// A simple typing indicator component that displays animated dots to indicate
/// that the assistant is "thinking" or generating a response.
/// </summary>
public partial class MudTypingIndicator : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}
