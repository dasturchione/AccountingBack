using Application.Abstractions.Integration;
using Application.Features.Edocs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Integration.EImzo.Services;

namespace Integration.Edocs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdocsIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EdocsOptions>(configuration.GetSection(EdocsOptions.SectionName));
        services.AddHttpClient(EdocsOptions.HttpClientName);
        services.AddScoped<EdocsResponseSanitizer>();
        services.AddScoped<IEdocsChallengeStore>(sp =>
            sp.GetRequiredService<EImzoChallengeStore>());
        services.AddScoped<IEdocsClient, EdocsClient>();
        services.AddScoped<IEdocsIntegrationService, EdocsIntegrationService>();
        return services;
    }
}
