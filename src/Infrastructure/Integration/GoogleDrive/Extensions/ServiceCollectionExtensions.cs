using Integration.GoogleDrive.Configs;
using Integration.GoogleDrive.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Integration.GoogleDrive.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleDriveIntegration(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<GoogleDriveSettings>, GoogleDriveSettingsValidator>();
        services.AddOptions<GoogleDriveSettings>()
            .Bind(configuration.GetSection("GoogleDrive"))
            .ValidateOnStart();
        services.AddScoped<IGoogleDriveUploader, GoogleDriveUploader>();

        return services;
    }
}
