using Domain.Entities;

namespace Application.Features.Notifications;

public interface INotificationEmailDispatcher
{
    Task DispatchAsync(Notification notification, NotificationDelivery delivery, CancellationToken ct = default);
    Task DispatchPendingAsync(CancellationToken ct = default);
}
