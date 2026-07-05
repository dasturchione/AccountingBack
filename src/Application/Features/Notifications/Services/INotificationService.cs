using SharedKernel.Results;

namespace Application.Features.Notifications;

public interface INotificationService
{
    Task<Result<long>> CreateAsync(CreateNotificationRequest request, CancellationToken ct = default);
    Task<Result<NotificationListResponse>> GetForUserAsync(NotificationQuery query, CancellationToken ct = default);
    Task<Result<int>> GetUnreadCountAsync(CancellationToken ct = default);
    Task<Result> MarkAsReadAsync(long notificationId, CancellationToken ct = default);
    Task<Result> MarkAllAsReadAsync(CancellationToken ct = default);
}
