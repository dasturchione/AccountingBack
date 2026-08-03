using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Integration.Edocs.Http;
using System.Net;

namespace Integration.Edocs.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdocsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<EdocsOptions>, EdocsOptionsValidator>());

        services.AddOptions<EdocsOptions>()
            .Bind(configuration.GetSection(EdocsOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<EdocsTokenCache>();

        return services;
    }

    public static IServiceCollection AddEdocsHttpClient(this IServiceCollection services)
    {
        services.AddTransient<EdocsAuthorizationHandler>();
        services.AddTransient<EdocsGetRetryHandler>();

        // Auth handler'siz, faqat /authId va /login (ЭЦП handshake) uchun — o'z-o'zini
        // avtorizatsiya qilish (recursion) ehtimolini yo'q qilish uchun alohida named client.
        services.AddHttpClient(EdocsHttpClientNames.AuthClient, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<EdocsOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
                | DecompressionMethods.Deflate
                | DecompressionMethods.Brotli
        });

        services.AddHttpClient(EdocsHttpClientNames.Client, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<EdocsOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
                    | DecompressionMethods.Deflate
                    | DecompressionMethods.Brotli
            })
            // Tartib: auth tashqarida, retry ichkarida — AslBelgi bilan bir xil.
            .AddHttpMessageHandler<EdocsAuthorizationHandler>()
            .AddHttpMessageHandler<EdocsGetRetryHandler>();

        return services;
    }
}
