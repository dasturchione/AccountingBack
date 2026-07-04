using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;
using Scrutor;

namespace Application.Features.Cmn.Taxes.Extensions;

/// <summary>
/// Registers Tax module services and validators.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaxModule(this IServiceCollection services)
    {
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<ITaxResolverService, TaxResolverService>();
        services.AddScoped<ITaxCalculationService, TaxCalculationService>();
        services.AddScoped<Application.Features.Cmn.Taxes.Integration.Services.ITaxIntegrationService, Application.Features.Cmn.Taxes.Integration.Services.TaxIntegrationService>();

        services.Scan(scan => scan
            .FromAssemblyOf<TaxFeatureRegistration>()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(ICriteriaBuilder<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IProjectionBuilder<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
