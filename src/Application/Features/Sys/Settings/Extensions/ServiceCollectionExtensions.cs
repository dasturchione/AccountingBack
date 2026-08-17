using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Settings.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<ISettingService, SettingService>();

        return services;
    }
}
