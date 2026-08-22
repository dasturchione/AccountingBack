using Integration.GoogleDrive.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Integration.GoogleDrive.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleDriveIntegration(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<GoogleDriveSettings>, GoogleDriveOptionsValidator>();
        services.AddOptions<GoogleDriveSettings>()
            .Bind(configuration.GetSection(GoogleDriveSettings.SectionName))
            .ValidateOnStart();
        services.AddScoped<IGoogleDriveUploader, GoogleDriveUploader>();
        services.AddSingleton<IGoogleDriveServiceFactory, GoogleDriveServiceFactory>();

        return services;
    }
}
