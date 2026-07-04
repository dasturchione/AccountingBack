using Application.Abstractions.Integration;
using Integration.CentralBank.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.CentralBank.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCentralBankIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CentralBankSettings>(configuration.GetSection("CentralBank"));
        services.AddHttpClient<ICurrencyRateProvider, CentralBankCurrencyRateProvider>();
        return services;
    }
}
