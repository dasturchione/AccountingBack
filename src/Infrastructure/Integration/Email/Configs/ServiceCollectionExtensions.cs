using Application.Abstractions.Integration;
using Integration.Email.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Email.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddScoped<IEmailSender, EmailSender>();
        return services;
    }
}
