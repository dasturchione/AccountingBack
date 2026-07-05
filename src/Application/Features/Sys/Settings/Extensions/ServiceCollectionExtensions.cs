using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Application.Features.Settings.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<ISettingService, SettingService>();

        services.Scan(scan => scan
            .FromAssemblyOf<SettingFeatureRegistration>()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
