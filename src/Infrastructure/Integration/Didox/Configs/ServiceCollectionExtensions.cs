using Integration.Didox.Http;
using Integration.Didox.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Integration.Didox.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDidoxConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<DidoxOptions>, DidoxOptionsValidator>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<DidoxTokenStorageOptions>, DidoxTokenStorageOptionsValidator>());

        services.AddOptions<DidoxOptions>()
            .Bind(configuration.GetSection(DidoxOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<DidoxTokenStorageOptions>()
            .Bind(configuration.GetSection(DidoxTokenStorageOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<DidoxTokenCache>();
        services.AddSingleton<DidoxTimestampClient>();

        return services;
    }

    public static IServiceCollection AddDidoxHttpClient(this IServiceCollection services)
    {
        services.AddTransient<DidoxAuthorizationHandler>();
        services.AddTransient<DidoxGetRetryHandler>();

        services.AddHttpClient(DidoxHttpClientNames.AuthClient, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<DidoxOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));

            if (!options.UsePartnerlessLegacyApi && !string.IsNullOrWhiteSpace(options.PartnerToken))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Partner-Authorization",
                    options.PartnerToken);
            }
        });

        services.AddHttpClient(DidoxHttpClientNames.Client, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<DidoxOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            })
            .AddHttpMessageHandler<DidoxAuthorizationHandler>()
            .AddHttpMessageHandler<DidoxGetRetryHandler>();

        return services;
    }
}
