using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Integration.AslBelgi.Http;

namespace Integration.AslBelgi.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAslBelgiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<AslBelgiOptions>, AslBelgiOptionsValidator>());

        services.AddOptions<AslBelgiOptions>()
            .Bind(configuration.GetSection(AslBelgiOptions.SectionName))
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddAslBelgiHttpClient(this IServiceCollection services)
    {
        services.AddTransient<AslBelgiAuthorizationHandler>();
        services.AddTransient<AslBelgiGetRetryHandler>();

        services.AddHttpClient(AslBelgiHttpClientNames.Client, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AslBelgiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            })
            // Tartib: auth tashqarida, retry ichkarida. Authorization sarlavhasi
            // bir marta qo'yiladi va AslBelgiGetRetryHandler uni har bir qayta
            // urinish nusxasiga ko'chiradi.
            .AddHttpMessageHandler<AslBelgiAuthorizationHandler>()
            .AddHttpMessageHandler<AslBelgiGetRetryHandler>();

        return services;
    }
}
