using Application.Abstractions.Integration;
using Application.Abstractions.Integration.Faktura;
using Integration.Faktura.Http;
using Integration.Faktura.Persistence;
using Integration.Faktura.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Net;

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

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<FakturaAuthSessionStorageOptions>, FakturaAuthSessionStorageOptionsValidator>());

        services.AddOptions<FakturaAuthSessionStorageOptions>()
            .Bind(configuration.GetSection(FakturaAuthSessionStorageOptions.SectionName))
            .ValidateOnStart();

        services.AddTransient<FakturaAuthorizationHandler>();
        services.AddSingleton<IFakturaTokenService, FakturaTokenService>();
        services.AddScoped<IFakturaAuthSessionStore, FakturaAuthSessionStore>();

        // Token endpointi: handler'siz, aks holda rekursiya bo'ladi.
        services.AddHttpClient(FakturaHttpClientNames.AuthClient, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FakturaOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip
                | DecompressionMethods.Deflate
                | DecompressionMethods.Brotli
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
