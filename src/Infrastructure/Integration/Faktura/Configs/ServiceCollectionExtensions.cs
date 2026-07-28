using Application.Abstractions.Integration;
using Integration.Faktura.Http;
using Integration.Faktura.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Integration.Faktura.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFaktura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<FakturaOptions>, FakturaOptionsValidator>());

        services.AddOptions<FakturaOptions>()
            .Bind(configuration.GetSection(FakturaOptions.SectionName))
            .ValidateOnStart();

        services.AddTransient<FakturaAuthorizationHandler>();

        // Token endpointi: handler'siz, aks holda rekursiya bo'ladi.
        services.AddHttpClient(FakturaHttpClientNames.AuthClient, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FakturaOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
        });

        services.AddHttpClient(FakturaHttpClientNames.Client, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FakturaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            })
            .AddHttpMessageHandler<FakturaAuthorizationHandler>();

        services.AddSingleton<IFakturaService, FakturaService>();

        return services;
    }
}
