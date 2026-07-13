using Application.Abstractions.Integration;
using Application.Features.Integration.EImzo;
using Application.Features.Integration.EImzo.Policies;
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

        // Shared E-IMZO signature relay foundation. Registered standalone — it is deliberately NOT
        // wired into the Didox / E-DOCS / Asl Belgisi provider flows, and takes no IEImzoSigner.
        services.Configure<EImzoSignatureRelayOptions>(configuration.GetSection("EImzoSignatureRelay"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EImzoSignatureRelayOptions>>().Value);
        services.AddScoped<IEImzoProviderSignaturePolicy, EdocsEImzoSignaturePolicy>();
        services.AddScoped<IEImzoProviderSignaturePolicy, DidoxEImzoSignaturePolicy>();
        services.AddScoped<IEImzoProviderSignaturePolicy, AslBelgiEImzoSignaturePolicy>();
        services.AddScoped<EImzoChallengeStore>();
        services.AddScoped<IEImzoSignatureChallengeStore>(sp =>
            sp.GetRequiredService<EImzoChallengeStore>());
        services.AddScoped<IEImzoSignatureRelay, EImzoSignatureRelay>();

        return services;
    }
}
