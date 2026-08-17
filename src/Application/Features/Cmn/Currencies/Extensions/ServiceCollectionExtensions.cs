using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Cmn.Currencies.Extensions;

/// <summary>
/// Registers Currency module foundation services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds currency services.
    /// </summary>
    public static IServiceCollection AddCurrencyModule(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyService, CurrencyService>();

        return services;
    }
}
