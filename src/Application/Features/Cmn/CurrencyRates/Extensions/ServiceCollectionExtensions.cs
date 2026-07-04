using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Application.Features.Cmn.CurrencyRates.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyRateModule(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRateService, CurrencyRateService>();

        services.Scan(scan => scan
            .FromAssemblyOf<CurrencyRateFeatureRegistration>()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
