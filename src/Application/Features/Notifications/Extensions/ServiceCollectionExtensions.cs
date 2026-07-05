using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Application.Features.Notifications.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationEmailDispatcher, NotificationEmailDispatcher>();

        services.Scan(scan => scan
            .FromAssemblyOf<NotificationFeatureRegistration>()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
