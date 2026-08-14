using Application.Features.Notifications;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class NotificationReadRepository : INotificationReadRepository
{
    private readonly AppDbContext _context;

    public NotificationReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationReadPageResult> GetForUserAsync(int userId, int? explicitOrganizationId, IReadOnlyCollection<int> allowedOrganizationIds, NotificationQuery query, CancellationToken ct = default)
    {
        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize is > 0 ? query.PageSize.Value : 20;

        var baseQuery = BuildVisibleNotificationsQuery(userId, explicitOrganizationId, allowedOrganizationIds, query.TypeId);
        var projectedQuery = BuildNotificationReadProjection(baseQuery, userId);

        if (query.IsRead.HasValue)
            projectedQuery = projectedQuery.Where(x => x.IsRead == query.IsRead.Value);

        var totalCount = await projectedQuery.CountAsync(ct);
        var items = await projectedQuery
            .OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new NotificationReadPageResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public Task<int> GetUnreadCountAsync(int userId, int? explicitOrganizationId, IReadOnlyCollection<int> allowedOrganizationIds, CancellationToken ct = default)
    {
        var query =
            from notification in BuildVisibleNotificationsQuery(userId, explicitOrganizationId, allowedOrganizationIds, null)
            join read in _context.NotificationReads.AsNoTracking().Where(x => x.UserId == userId)
                on notification.Id equals read.NotificationId into reads
            from read in reads.DefaultIfEmpty()
            where read == null
            select notification.Id;

        return query.CountAsync(ct);
    }

    public Task<bool> IsVisibleAsync(long notificationId, int userId, IReadOnlyCollection<int> allowedOrganizationIds, CancellationToken ct = default) =>
        BuildVisibleNotificationsQuery(userId, null, allowedOrganizationIds, null)
            .AnyAsync(x => x.Id == notificationId, ct);

    public Task<List<long>> GetUnreadNotificationIdsAsync(int userId, IReadOnlyCollection<int> allowedOrganizationIds, CancellationToken ct = default)
    {
        var query =
            from notification in BuildVisibleNotificationsQuery(userId, null, allowedOrganizationIds, null)
            join read in _context.NotificationReads.AsNoTracking().Where(x => x.UserId == userId)
                on notification.Id equals read.NotificationId into reads
            from read in reads.DefaultIfEmpty()
            where read == null
            orderby notification.CreatedDate descending
            select notification.Id;

        return query.ToListAsync(ct);
    }

    private IQueryable<Notification> BuildVisibleNotificationsQuery(int userId, int? explicitOrganizationId, IReadOnlyCollection<int> allowedOrganizationIds, short? typeId)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();

        query = query.Where(x =>
            (x.UserId == userId && (!x.OrganizationId.HasValue || (explicitOrganizationId.HasValue
                ? x.OrganizationId == explicitOrganizationId.Value
                : allowedOrganizationIds.Contains(x.OrganizationId.Value))))
            || (x.UserId == null && (x.OrganizationId == null || (explicitOrganizationId.HasValue
                ? x.OrganizationId == explicitOrganizationId.Value
                : allowedOrganizationIds.Contains(x.OrganizationId.Value)))));

        if (typeId.HasValue)
            query = query.Where(x => x.TypeId == typeId.Value);

        return query;
    }

    private IQueryable<NotificationReadListItem> BuildNotificationReadProjection(IQueryable<Notification> notifications, int userId)
    {
        return
            from notification in notifications
            join read in _context.NotificationReads.AsNoTracking().Where(x => x.UserId == userId)
                on notification.Id equals read.NotificationId into reads
            from read in reads.DefaultIfEmpty()
            select new NotificationReadListItem
            {
                Id = notification.Id,
                TypeId = notification.TypeId,
                Title = notification.Title,
                Body = notification.Body,
                IsRead = read != null,
                ReadAt = read != null ? read.ReadAt : null,
                Link = notification.Link,
                CreatedDate = notification.CreatedDate
            };
    }
}
