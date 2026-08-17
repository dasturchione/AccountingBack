using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Cmn.CurrencyRevaluations.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyRevaluationModule(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRevaluationService, CurrencyRevaluationService>();
        return services;
    }
}
