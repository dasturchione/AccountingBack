using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Cmn.CurrencyRates.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyRateModule(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRateService, CurrencyRateService>();

        return services;
    }
}
