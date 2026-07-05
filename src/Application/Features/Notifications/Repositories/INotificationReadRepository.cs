namespace Application.Features.Notifications;

public interface INotificationReadRepository
{
    Task<NotificationReadPageResult> GetForUserAsync(int userId, int? organizationId, NotificationQuery query, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(int userId, int? organizationId, CancellationToken ct = default);
    Task<bool> IsVisibleAsync(long notificationId, int userId, int? organizationId, CancellationToken ct = default);
    Task<List<long>> GetUnreadNotificationIdsAsync(int userId, int? organizationId, CancellationToken ct = default);
}

public sealed class NotificationReadPageResult
{
    public IReadOnlyCollection<NotificationReadListItem> Items { get; init; } = [];
    public int TotalCount { get; init; }
}

public sealed class NotificationReadListItem
{
    public long Id { get; init; }
    public short TypeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public string? Link { get; init; }
    public DateTime CreatedDate { get; init; }
}
