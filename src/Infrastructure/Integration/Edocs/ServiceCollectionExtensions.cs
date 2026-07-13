using Application.Abstractions.Integration;
using Application.Features.Edocs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Integration.EImzo.Services;

namespace Integration.Edocs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdocsIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EdocsOptions>(configuration.GetSection(EdocsOptions.SectionName));
        // Session TTL policy resolved from the existing Edocs:TokenTtlMinutes key; invalid or
        // out-of-range values fall back to the safe default inside FromMinutes.
        services.AddSingleton(sp => EdocsSessionOptions.FromMinutes(
            sp.GetRequiredService<IOptions<EdocsOptions>>().Value.TokenTtlMinutes));
        services.AddHttpClient(EdocsOptions.HttpClientName)
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<EdocsOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 120));
            });
        services.AddScoped<EdocsResponseSanitizer>();
        services.AddScoped<IEdocsChallengeStore>(sp =>
            sp.GetRequiredService<EImzoChallengeStore>());
        services.AddScoped<IEdocsClient, EdocsClient>();
        services.AddScoped<IEdocsIntegrationService, EdocsIntegrationService>();
        return services;
    }
}
