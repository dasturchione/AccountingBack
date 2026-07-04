using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Application.Features.Cmn.CurrencyRevaluations.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyRevaluationModule(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRevaluationService, CurrencyRevaluationService>();
        services.Scan(scan => scan.FromAssemblyOf<CurrencyRevaluationFeatureRegistration>()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces().WithScopedLifetime());
        return services;
    }
}
