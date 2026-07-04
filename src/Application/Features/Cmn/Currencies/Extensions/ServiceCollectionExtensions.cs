using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Application.Features.Cmn.Currencies.Extensions;

/// <summary>
/// Registers Currency module foundation services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds currency validators and future feature scaffolding.
    /// </summary>
    public static IServiceCollection AddCurrencyModule(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyService, CurrencyService>();

        services.Scan(scan => scan
            .FromAssemblyOf<CurrencyFeatureRegistration>()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
