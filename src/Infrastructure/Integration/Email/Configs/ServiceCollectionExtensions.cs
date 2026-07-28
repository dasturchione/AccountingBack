using Application.Abstractions.Integration;
using Integration.Email.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Integration.Email.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>());

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<IEmailSender, EmailSender>();
        return services;
    }
}
