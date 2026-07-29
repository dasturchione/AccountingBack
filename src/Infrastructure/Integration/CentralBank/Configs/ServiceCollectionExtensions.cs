using Application.Abstractions.Integration;
using Integration.CentralBank.Http;
using Integration.CentralBank.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Integration.CentralBank.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCentralBankIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<CentralBankOptions>, CentralBankOptionsValidator>());

        services.AddOptions<CentralBankOptions>()
            .Bind(configuration.GetSection(CentralBankOptions.SectionName))
            .ValidateOnStart();

        services.AddTransient<CentralBankRetryHandler>();

        // Markaziy bank API'si ochiq: authorization handler kerak emas.
        services.AddHttpClient(CentralBankHttpClientNames.Client, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<CentralBankOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            })
            .AddHttpMessageHandler<CentralBankRetryHandler>();

        services.AddTransient<ICurrencyRateProvider, CentralBankCurrencyRateProvider>();

        return services;
    }
}
