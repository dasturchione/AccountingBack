using Integration.GoogleDrive.Configs;
using Integration.GoogleDrive.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.GoogleDrive.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleDriveIntegration(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.Configure<GoogleDriveSettings>(configuration.GetSection("GoogleDrive"));
        services.AddScoped<IGoogleDriveUploader, GoogleDriveUploader>();

        return services;
    }
}
