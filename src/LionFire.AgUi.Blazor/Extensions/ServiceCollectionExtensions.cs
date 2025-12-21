using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LionFire.AgUi.Blazor;

/// <summary>
/// Extension methods for registering AG-UI Blazor core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the AG-UI Blazor core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method registers the following services:
    /// <list type="bullet">
    ///   <item><description><see cref="ITokenCostCalculator"/> - Token cost calculation service</description></item>
    ///   <item><description><see cref="IKeyboardShortcutService"/> - Keyboard shortcut management service</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs or Startup.cs
    /// builder.Services.AddAgUiBlazor();
    /// </code>
    /// </example>
    public static IServiceCollection AddAgUiBlazor(this IServiceCollection services)
    {
        services.TryAddSingleton<ITokenCostCalculator, TokenCostCalculator>();
        services.TryAddSingleton<IKeyboardShortcutService>(sp =>
        {
            var service = new KeyboardShortcutService();
            service.RegisterDefaults();
            return service;
        });

        return services;
    }

    /// <summary>
    /// Adds the AG-UI Blazor core services with custom token cost options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure token cost options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddAgUiBlazor(options =>
    /// {
    ///     options.DefaultRates = new ModelCostRates(1.0m, 2.0m);
    ///     options.ModelRates["custom-model"] = new ModelCostRates(5.0m, 10.0m);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAgUiBlazor(
        this IServiceCollection services,
        Action<TokenCostOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.TryAddSingleton<ITokenCostCalculator, TokenCostCalculator>();

        return services;
    }
}
