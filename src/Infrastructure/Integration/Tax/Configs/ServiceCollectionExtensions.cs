using Application.Abstractions.Integration;
using Integration.Tax.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Tax.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaxIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TaxIntegrationSettings>(configuration.GetSection("TaxIntegration"));
        services.AddHttpClient("TaxIntegration");
        services.AddScoped<ITaxProviderFactory, TaxProviderFactory>();
        services.AddScoped<ITaxProvider, MxikTaxProvider>();
        services.AddScoped<ITaxProvider, SoliqApiTaxProvider>();
        services.AddScoped<ITaxProvider, EFakturaTaxProvider>();
        services.AddScoped<ITaxProvider, DidoxTaxProvider>();
        return services;
    }
}
