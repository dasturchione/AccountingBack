using Application.Features.Integration.Edo;
using Application.Abstractions.Integration.Edo;
using Integration.Edo.Auth;
using Integration.Edo.Persistence;
using Integration.Edo.Providers;
using Integration.Didox.Historical;
using Integration.Edocs.Historical;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Edo.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdoProviderRegistry(this IServiceCollection services)
    {
        services.AddScoped<IEdoProvider, DidoxEdoProvider>();
        services.AddScoped<IEdoProvider, FakturaEdoProvider>();
        services.AddScoped<IEdoProvider, EdocsEdoProvider>();
        services.AddScoped<IEdoHistoricalDocumentSource, DidoxHistoricalDocumentSource>();
        services.AddScoped<IEdoHistoricalDocumentSource, EdocsHistoricalDocumentSource>();
        services.AddScoped<IEdoProviderRegistry, EdoProviderRegistry>();
        services.AddScoped<IEdoProviderConfiguration, EdoProviderConfiguration>();
        services.AddScoped<IActiveEdoProviderStore, OrganizationEdoProviderStore>();
        services.AddScoped<IActiveEdoProviderResolver, OrganizationActiveEdoProviderResolver>();
        services.AddScoped<IEdoAuthSigningSessionStore, EdoAuthSigningSessionStore>();
        services.AddScoped<IEdoAuthCredentialValidator, EdoAuthCredentialValidator>();
        services.AddScoped<IEdoDocumentStore, EdoDocumentStore>();
        services.AddScoped<IEdoImportStore, EdoImportStore>();
        services.AddScoped<IEdoIdempotencyStore, EdoIdempotencyStore>();
        services.AddScoped<IEdoIdempotencyService, EdoIdempotencyService>();
        services.AddScoped<IEdoDocumentSigningSessionStore, EdoDocumentSigningSessionStore>();
        services.AddScoped<IEdoReconciliationService, Reconciliation.EdoReconciliationService>();
        services.AddScoped<Integration.Didox.Facturas.DidoxEdoOperations>();
        services.AddScoped<Integration.Edocs.Facturas.EdocsEdoOperations>();
        services.AddScoped<Integration.Faktura.Edo.FakturaEdoOperations>();

        return services;
    }
}
