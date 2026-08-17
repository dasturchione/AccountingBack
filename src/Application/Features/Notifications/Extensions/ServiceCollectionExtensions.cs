using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Notifications.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationEmailDispatcher, NotificationEmailDispatcher>();

        return services;
    }
}
