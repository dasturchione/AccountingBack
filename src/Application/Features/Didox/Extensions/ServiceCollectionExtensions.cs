using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Didox.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDidoxPersistenceModule(this IServiceCollection services)
    {
        services.AddScoped<DidoxPersistenceService>();
        services.AddScoped<IDidoxMxikCatalogService>(sp => sp.GetRequiredService<DidoxPersistenceService>());
        services.AddScoped<IDidoxOriginService>(sp => sp.GetRequiredService<DidoxPersistenceService>());
        services.AddScoped<IDidoxVatRegStatusService>(sp => sp.GetRequiredService<DidoxPersistenceService>());
        services.AddScoped<IUnitDidoxPackageService>(sp => sp.GetRequiredService<DidoxPersistenceService>());
        services.AddScoped<ICounterpartyDidoxProfileService>(sp => sp.GetRequiredService<DidoxPersistenceService>());
        services.AddScoped<IProductDidoxProfileService>(sp => sp.GetRequiredService<DidoxPersistenceService>());
        services.AddScoped<IProductTableDidoxOriginService>(sp => sp.GetRequiredService<DidoxPersistenceService>());
        return services;
    }
}
