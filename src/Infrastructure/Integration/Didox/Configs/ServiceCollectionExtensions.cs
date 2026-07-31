using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Integration.Didox.Http;
using Integration.Didox.Services;

namespace Integration.Didox.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDidoxConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<DidoxOptions>, DidoxOptionsValidator>());

        services.AddOptions<DidoxOptions>()
            .Bind(configuration.GetSection(DidoxOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<DidoxTokenCache>();

        // POST /v1/dsvs/timestamp — DidoxAuthService (login) va DidoxFacturaService
        // (hujjat imzolash, 7.3-bosqich) ikkalasi ham shu yagona klientdan foydalanadi.
        services.AddSingleton<DidoxTimestampClient>();

        return services;
    }

    public static IServiceCollection AddDidoxHttpClient(this IServiceCollection services)
    {
        services.AddTransient<DidoxAuthorizationHandler>();
        services.AddTransient<DidoxGetRetryHandler>();

        // Auth handler'siz, faqat /v1/dsvs/timestamp va /v1/auth/{taxId}/token/{locale}
        // (ЭЦП handshake) uchun — o'z-o'zini avtorizatsiya qilish (recursion) ehtimolini
        // yo'q qilish uchun alohida named client (EdocsHttpClientNames.AuthClient bilan
        // bir xil naqsh). Partner-Authorization bu yerda ham HAR SO'ROVDA kerak
        // (INT_DIDOX.md §2.1), lekin DidoxAuthorizationHandler bu client'ga ulanmagani
        // uchun DefaultRequestHeaders orqali, client qurilish paytida qo'yiladi.
        services.AddHttpClient(DidoxHttpClientNames.AuthClient, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<DidoxOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));

            // PartnerToken ValidateOnStart'da tekshirilmaydi (DidoxOptionsValidator) —
            // shuning uchun bu yerda, RUNTIME'da (har CreateClient chaqiruvida) aniq
            // xato bilan tekshiriladi.
            if (string.IsNullOrWhiteSpace(options.PartnerToken))
            {
                throw new InvalidOperationException(
                    "Didox:PartnerToken is not configured — set it in appsettings.json (obtained manually from the Didox account manager, INT_DIDOX.md §1.3).");
            }

            client.DefaultRequestHeaders.TryAddWithoutValidation("Partner-Authorization", options.PartnerToken);
        });

        services.AddHttpClient(DidoxHttpClientNames.Client, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<DidoxOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            })
            // Tartib: auth tashqarida, retry ichkarida — AslBelgi/Edocs bilan bir xil.
            // Partner-Authorization va user-key ikkalasi ham DidoxAuthorizationHandler
            // ichida qo'yiladi (u yerda organizationId ham tekshiriladi).
            .AddHttpMessageHandler<DidoxAuthorizationHandler>()
            .AddHttpMessageHandler<DidoxGetRetryHandler>();

        return services;
    }
}
