using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.AgUi.Blazor.MudBlazor;

/// <summary>
/// Extension methods for registering MudBlazor AG-UI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the MudBlazor AG-UI services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method registers the following services:
    /// <list type="bullet">
    ///   <item><description><see cref="IMarkdownRenderer"/> - Markdown rendering using Markdig</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs or Startup.cs
    /// builder.Services.AddAgUiMudBlazor();
    /// </code>
    /// </example>
    public static IServiceCollection AddAgUiMudBlazor(this IServiceCollection services)
    {
        services.AddSingleton<IMarkdownRenderer, MarkdigRenderer>();

        return services;
    }
}
