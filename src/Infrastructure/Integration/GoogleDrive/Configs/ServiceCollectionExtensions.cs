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
        services.AddSingleton<IValidateOptions<GoogleDriveOptions>, GoogleDriveOptionsValidator>();
        services.AddOptions<GoogleDriveOptions>()
            .Bind(configuration.GetSection(GoogleDriveOptions.SectionName))
            .ValidateOnStart();
        services.AddScoped<IGoogleDriveUploader, GoogleDriveUploader>();

        return services;
    }
}
