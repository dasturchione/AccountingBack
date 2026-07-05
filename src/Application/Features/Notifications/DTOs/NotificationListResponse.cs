namespace Application.Features.Notifications;

public sealed class NotificationListResponse
{
    public IReadOnlyCollection<NotificationDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int UnreadCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
