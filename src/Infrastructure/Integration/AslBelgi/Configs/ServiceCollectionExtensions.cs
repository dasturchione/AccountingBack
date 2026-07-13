using Application.Features.Cmn.AslBelgi.Abstractions;
using Integration.AslBelgi.Abstractions;
using Integration.AslBelgi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.AslBelgi.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAslBelgiIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AslBelgiSettings>(configuration.GetSection("AslBelgi"));

        services.AddHttpClient("AslBelgi");
        services.AddHttpClient("AslBelgiAuth");

        services.AddScoped<IAslBelgiAuthClient, AslBelgiAuthClient>();
        services.AddScoped<IAslBelgiTokenProvider, AslBelgiTokenProvider>();
        services.AddScoped<IAslBelgiClient, AslBelgiClient>();
        services.AddScoped<IAslBelgiMarkingRepository, AslBelgiMarkingReadRepository>();
        services.AddSingleton<IAslBelgiOrganizationCapabilityResolver, OptionsAslBelgiOrganizationCapabilityResolver>();

        return services;
    }
}
