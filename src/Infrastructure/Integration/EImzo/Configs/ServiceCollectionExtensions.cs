using Application.Abstractions.Integration;
using Integration.EImzo.Services;
using Integration.EImzo.Services.Signers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.EImzo.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEImzoIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EImzoSettings>(configuration.GetSection("EImzo"));
        services.AddHttpClient("EImzoServer");
        services.AddScoped<ICertificateSigner, RsaSigner>();
        services.AddScoped<ICertificateSigner, GostSigner>();
        services.AddScoped<IEImzoSigner, EImzoSigner>();
        services.AddScoped<IEImzoVerifier, EImzoVerifier>();

        return services;
    }
}
