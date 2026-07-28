using Application.Abstractions.Integration;
using Integration.Tax.Http;
using Integration.Tax.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Integration.Tax.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaxIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TaxIntegrationOptions>, TaxIntegrationOptionsValidator>());

        services.AddOptions<TaxIntegrationOptions>()
            .Bind(configuration.GetSection(TaxIntegrationOptions.SectionName))
            .ValidateOnStart();

        services.AddTransient<TaxRetryHandler>();

        // Soliq provayderlari ochiq API: auth handler kerak emas.
        services.AddHttpClient(TaxHttpClientNames.Client, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<TaxIntegrationOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            })
            .AddHttpMessageHandler<TaxRetryHandler>();

        services.AddScoped<ITaxProviderFactory, TaxProviderFactory>();
        services.AddScoped<ITaxProvider, MxikTaxProvider>();
        services.AddScoped<ITaxProvider, SoliqApiTaxProvider>();
        services.AddScoped<ITaxProvider, EFakturaTaxProvider>();
        return services;
    }
}
