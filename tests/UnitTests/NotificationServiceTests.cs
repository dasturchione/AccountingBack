using System.Linq.Expressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Notifications;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;

namespace UnitTests;

public class NotificationServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldKeepInAppNotification_WhenEmailDeliveryFails()
    {
        var notifications = new List<Notification>();
        var deliveries = new List<NotificationDelivery>();
        var reads = new List<NotificationRead>();
        var users = new List<User>
        {
            new() { Id = 10, UserName = "aziz", FirstName = "Aziz", LastName = "Karimov", Email = "aziz@example.com", LanguageId = LanguageIdConst.UZ }
        };
        var types = new List<NotificationType>
        {
            new() { Id = 6, Code = "payment_due", Name = "Payment due", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var dispatcher = new NotificationEmailDispatcher(
            new NotificationQueryRepository<User>(users),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationQueryRepository<NotificationDelivery>(deliveries),
            new NotificationCommandRepository<NotificationDelivery>(deliveries),
            new FakeNotificationEmailSender
            {
                Result = SharedKernel.Results.Result.Failure(SharedKernel.Results.Error.Problem("Email.SendFailed", "SMTP timeout"))
            },
            NullLogger<NotificationEmailDispatcher>.Instance);

        var service = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>(deliveries),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>(types),
            dispatcher);

        var result = await service.CreateAsync(new CreateNotificationRequest
        {
            TypeId = 6,
            UserId = 10,
            OrganizationId = 8,
            Title = "Payment due",
            Body = "Pay before deadline",
            Channels = [NotificationChannel.InApp, NotificationChannel.Email]
        });

        Assert.True(result.IsSuccess);
        Assert.Single(notifications);
        Assert.Equal(2, deliveries.Count);
        Assert.Contains(deliveries, x => x.Channel == (short)NotificationChannel.InApp && x.Status == (short)NotificationDeliveryStatus.Sent);
        Assert.Contains(deliveries, x => x.Channel == (short)NotificationChannel.Email && x.Status == (short)NotificationDeliveryStatus.Failed && x.Error == "SMTP timeout");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateInAppAndEmailDeliveries_AndDispatchEmail()
    {
        var notifications = new List<Notification>();
        var deliveries = new List<NotificationDelivery>();
        var reads = new List<NotificationRead>();
        var types = new List<NotificationType>
        {
            new() { Id = 5, Code = "doc_approved", Name = "Approved", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var dispatcher = new FakeNotificationEmailDispatcher();

        var service = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>(deliveries),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>(types),
            dispatcher);

        var result = await service.CreateAsync(new CreateNotificationRequest
        {
            TypeId = 5,
            UserId = 10,
            OrganizationId = 8,
            Title = "Approved",
            Body = "Document approved",
            Channels = [NotificationChannel.InApp, NotificationChannel.Email]
        });

        Assert.True(result.IsSuccess);
        Assert.Single(notifications);
        Assert.Equal(2, deliveries.Count);
        Assert.Contains(deliveries, x => x.Channel == (short)NotificationChannel.InApp && x.Status == (short)NotificationDeliveryStatus.Sent);
        Assert.Contains(deliveries, x => x.Channel == (short)NotificationChannel.Email && x.Status == (short)NotificationDeliveryStatus.Pending);
        Assert.Single(dispatcher.DispatchCalls);
        Assert.Equal(notifications.Single().Id, dispatcher.DispatchCalls.Single().Notification.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldDefaultToInApp_WhenChannelsAreMissing()
    {
        var notifications = new List<Notification>();
        var deliveries = new List<NotificationDelivery>();
        var reads = new List<NotificationRead>();
        var types = new List<NotificationType>
        {
            new() { Id = 1, Code = "info", Name = "Info", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };

        var service = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>(deliveries),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>(types),
            new FakeNotificationEmailDispatcher());

        var result = await service.CreateAsync(new CreateNotificationRequest
        {
            TypeId = 1,
            UserId = 10,
            OrganizationId = 8,
            Title = "Info",
            Body = "Body"
        });

        Assert.True(result.IsSuccess);
        Assert.Single(deliveries);
        Assert.Equal((short)NotificationChannel.InApp, deliveries.Single().Channel);
    }

    [Fact]
    public async Task GetForUserAsync_ShouldReturnPersonalBroadcastAndGlobalNotifications()
    {
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = 10, OrganizationId = 8, TypeId = 1, Title = "Personal", Body = "Mine", StateId = StateIdConst.ACTIVE, CreatedDate = new DateTime(2026, 7, 5, 9, 0, 0) },
            new() { Id = 2, UserId = null, OrganizationId = 8, TypeId = 2, Title = "Broadcast", Body = "Org", StateId = StateIdConst.ACTIVE, CreatedDate = new DateTime(2026, 7, 5, 8, 0, 0) },
            new() { Id = 3, UserId = null, OrganizationId = null, TypeId = 3, Title = "Global", Body = "All", StateId = StateIdConst.ACTIVE, CreatedDate = new DateTime(2026, 7, 5, 7, 0, 0) },
            new() { Id = 4, UserId = 11, OrganizationId = 8, TypeId = 1, Title = "Other User", Body = "Nope", StateId = StateIdConst.ACTIVE, CreatedDate = new DateTime(2026, 7, 5, 6, 0, 0) },
            new() { Id = 5, UserId = null, OrganizationId = 9, TypeId = 1, Title = "Other Org", Body = "Nope", StateId = StateIdConst.ACTIVE, CreatedDate = new DateTime(2026, 7, 5, 5, 0, 0) }
        };

        var reads = new List<NotificationRead>
        {
            new() { Id = 50, NotificationId = 3, UserId = 10, ReadAt = new DateTime(2026, 7, 5, 9, 30, 0), CreatedDate = new DateTime(2026, 7, 5, 9, 30, 0) }
        };

        var types = new List<NotificationType>
        {
            new() { Id = 1, Code = "info", Name = "Info", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            new() { Id = 2, Code = "warning", Name = "Warning", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            new() { Id = 3, Code = "error", Name = "Error", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };

        var service = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>([]),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>(types),
            new FakeNotificationEmailDispatcher());

        var result = await service.GetForUserAsync(new NotificationQuery { Page = 1, PageSize = 10 });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(2, result.Value.UnreadCount);
        Assert.Equal([1L, 2L, 3L], result.Value.Items.Select(x => x.Id).ToArray());
        Assert.False(result.Value.Items.Single(x => x.Id == 2).IsRead);
        Assert.True(result.Value.Items.Single(x => x.Id == 3).IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldFail_ForNotificationOutsideCurrentUsersScope()
    {
        var notifications = new List<Notification>
        {
            new() { Id = 100, UserId = 11, OrganizationId = 8, TypeId = 1, Title = "Other", Body = "Other", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var reads = new List<NotificationRead>();

        var service = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>([]),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>([]),
            new FakeNotificationEmailDispatcher());

        var result = await service.MarkAsReadAsync(100);

        Assert.False(result.IsSuccess);
        Assert.Equal("Notification.NotFound", result.Error.Code);
        Assert.Empty(reads);
    }

    [Fact]
    public async Task BroadcastRead_ShouldBePerUser_NotShared()
    {
        var notifications = new List<Notification>
        {
            new() { Id = 2, UserId = null, OrganizationId = 8, TypeId = 1, Title = "Broadcast", Body = "Org", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var reads = new List<NotificationRead>();

        var azizService = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>([]),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>([]),
            new FakeNotificationEmailDispatcher());

        var boburService = CreateService(
            new NotificationTestUserContext(userId: 11, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>([]),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>([]),
            new FakeNotificationEmailDispatcher());

        var azizRead = await azizService.MarkAsReadAsync(2);
        var boburUnreadCount = await boburService.GetUnreadCountAsync();
        var boburList = await boburService.GetForUserAsync(new NotificationQuery { Page = 1, PageSize = 10 });

        Assert.True(azizRead.IsSuccess);
        Assert.True(boburUnreadCount.IsSuccess);
        Assert.True(boburList.IsSuccess);
        Assert.Single(reads);
        Assert.Equal(1, boburUnreadCount.Value);
        Assert.False(boburList.Value.Items.Single().IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldNotCreateDuplicateReadRows()
    {
        var notifications = new List<Notification>
        {
            new() { Id = 7, UserId = 10, OrganizationId = 8, TypeId = 1, Title = "Personal", Body = "Mine", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var reads = new List<NotificationRead>();

        var service = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>([]),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>([]),
            new FakeNotificationEmailDispatcher());

        var first = await service.MarkAsReadAsync(7);
        var second = await service.MarkAsReadAsync(7);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Single(reads);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ShouldCreateReadRowsOnlyForCurrentUsersUnreadNotifications()
    {
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = 10, OrganizationId = 8, TypeId = 1, Title = "Personal", Body = "Mine", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            new() { Id = 2, UserId = null, OrganizationId = 8, TypeId = 1, Title = "Broadcast", Body = "Org", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            new() { Id = 3, UserId = null, OrganizationId = null, TypeId = 1, Title = "Global", Body = "All", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            new() { Id = 4, UserId = 11, OrganizationId = 8, TypeId = 1, Title = "Other", Body = "Nope", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var reads = new List<NotificationRead>
        {
            new() { Id = 100, NotificationId = 3, UserId = 10, ReadAt = DateTime.Today, CreatedDate = DateTime.Today }
        };

        var service = CreateService(
            new NotificationTestUserContext(userId: 10, organizationId: 8),
            new FakeNotificationReadRepository(notifications, reads),
            new NotificationQueryRepository<Notification>(notifications),
            new NotificationCommandRepository<Notification>(notifications),
            new NotificationCommandRepository<NotificationDelivery>([]),
            new NotificationQueryRepository<NotificationRead>(reads),
            new NotificationCommandRepository<NotificationRead>(reads),
            new NotificationQueryRepository<NotificationType>([]),
            new FakeNotificationEmailDispatcher());

        var result = await service.MarkAllAsReadAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, reads.Count);
        Assert.Contains(reads, x => x.NotificationId == 1 && x.UserId == 10);
        Assert.Contains(reads, x => x.NotificationId == 2 && x.UserId == 10);
        Assert.Contains(reads, x => x.NotificationId == 3 && x.UserId == 10);
        Assert.DoesNotContain(reads, x => x.NotificationId == 4 && x.UserId == 10);
    }

    private static NotificationService CreateService(
        IUserContext userContext,
        INotificationReadRepository notificationReadRepository,
        IQueryRepository<Notification> notificationQuery,
        ICommandRepository<Notification> notificationCommand,
        ICommandRepository<NotificationDelivery> notificationDeliveryCommand,
        IQueryRepository<NotificationRead> notificationReadQuery,
        ICommandRepository<NotificationRead> notificationReadCommand,
        IQueryRepository<NotificationType> notificationTypeQuery,
        INotificationEmailDispatcher notificationEmailDispatcher) =>
        new(
            userContext,
            notificationReadRepository,
            notificationQuery,
            notificationCommand,
            notificationDeliveryCommand,
            notificationReadQuery,
            notificationReadCommand,
            notificationTypeQuery,
            notificationEmailDispatcher,
            NullLogger<NotificationService>.Instance,
            new NotificationUnitOfWork());
}

file sealed class NotificationTestUserContext(int? userId, int? organizationId) : IUserContext
{
    public int? Id => userId;
    public int? RoleId => 1;
    public short? LanguageId => LanguageIdConst.EN;
    public int? OrganizationId => organizationId;
    public List<int> AllowedOrganizationIds => organizationId.HasValue ? [organizationId.Value] : [];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class NotificationUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class NotificationCommandRepository<TEntity>(List<TEntity> items) : ICommandRepository<TEntity> where TEntity : class
{
    public Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        AssignIdentityIfNeeded(entity);
        items.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            AssignIdentityIfNeeded(entity);
            items.Add(entity);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;

    private static void AssignIdentityIfNeeded(TEntity entity)
    {
        var property = typeof(TEntity).GetProperty("Id");
        if (property is null)
            return;

        if (property.PropertyType == typeof(long))
        {
            var currentValue = (long?)property.GetValue(entity) ?? 0L;
            if (currentValue == 0)
                property.SetValue(entity, DateTime.UtcNow.Ticks);

            return;
        }

        if (property.PropertyType == typeof(int))
        {
            var currentValue = (int?)property.GetValue(entity) ?? 0;
            if (currentValue == 0)
                property.SetValue(entity, (int)(DateTime.UtcNow.Ticks % int.MaxValue));
        }
    }
}

file sealed class NotificationQueryRepository<TEntity>(IEnumerable<TEntity> items) : IQueryRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _items = items as List<TEntity> ?? items.ToList();

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(_items.AsQueryable().Any(predicate));

    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
    {
        var query = _items.AsQueryable().Where(specification.Criteria);
        if (specification.OrderBy is not null)
            query = specification.OrderBy(query);

        return Task.FromResult(query.FirstOrDefault());
    }

    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
    {
        var query = _items.AsQueryable()
            .Where(specification.Criteria)
            .Select(specification.Selector);

        if (specification.ResultCriteria is not null)
            query = query.Where(specification.ResultCriteria);

        return Task.FromResult(query.FirstOrDefault());
    }

    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
    {
        var query = _items.AsQueryable().Where(specification.Criteria);
        if (specification.OrderBy is not null)
            query = specification.OrderBy(query);

        return Task.FromResult(query.ToList());
    }

    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
    {
        var query = _items.AsQueryable()
            .Where(specification.Criteria)
            .Select(specification.Selector);

        if (specification.OrderBy is not null)
            query = specification.OrderBy(query);

        if (specification.ResultCriteria is not null)
            query = query.Where(specification.ResultCriteria);

        return Task.FromResult(query.ToList());
    }

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default)
    {
        var query = _items.AsQueryable().Where(specification.Criteria);
        if (specification.OrderBy is not null)
            query = specification.OrderBy(query);

        var totalCount = query.Count();
        if (specification.Take.HasValue)
            query = query.Skip(specification.Skip).Take(specification.Take.Value);

        return Task.FromResult(new PagedList<TEntity>(query.ToList(), totalCount));
    }

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
    {
        var query = _items.AsQueryable()
            .Where(specification.Criteria)
            .Select(specification.Selector);

        if (specification.ResultCriteria is not null)
            query = query.Where(specification.ResultCriteria);

        if (specification.OrderBy is not null)
            query = specification.OrderBy(query);

        var totalCount = query.Count();
        if (specification.Take.HasValue)
            query = query.Skip(specification.Skip).Take(specification.Take.Value);

        return Task.FromResult(new PagedList<TResult>(query.ToList(), totalCount));
    }
}

file sealed class FakeNotificationReadRepository(
    List<Notification> notifications,
    List<NotificationRead> reads) : INotificationReadRepository
{
    public Task<NotificationReadPageResult> GetForUserAsync(int userId, int? organizationId, NotificationQuery query, CancellationToken ct = default)
    {
        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize is > 0 ? query.PageSize.Value : 20;

        var items = BuildVisibleNotifications(userId, organizationId)
            .Select(notification =>
            {
                var read = reads.FirstOrDefault(x => x.UserId == userId && x.NotificationId == notification.Id);
                return new NotificationReadListItem
                {
                    Id = notification.Id,
                    TypeId = notification.TypeId,
                    Title = notification.Title,
                    Body = notification.Body,
                    IsRead = read is not null,
                    ReadAt = read?.ReadAt,
                    Link = notification.Link,
                    CreatedDate = notification.CreatedDate
                };
            });

        if (query.TypeId.HasValue)
            items = items.Where(x => x.TypeId == query.TypeId.Value);

        if (query.IsRead.HasValue)
            items = items.Where(x => x.IsRead == query.IsRead.Value);

        var ordered = items.OrderByDescending(x => x.CreatedDate).ToList();

        return Task.FromResult(new NotificationReadPageResult
        {
            TotalCount = ordered.Count,
            Items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        });
    }

    public Task<int> GetUnreadCountAsync(int userId, int? organizationId, CancellationToken ct = default)
    {
        var count = BuildVisibleNotifications(userId, organizationId)
            .Count(notification => reads.All(x => x.UserId != userId || x.NotificationId != notification.Id));

        return Task.FromResult(count);
    }

    public Task<bool> IsVisibleAsync(long notificationId, int userId, int? organizationId, CancellationToken ct = default) =>
        Task.FromResult(BuildVisibleNotifications(userId, organizationId).Any(x => x.Id == notificationId));

    public Task<List<long>> GetUnreadNotificationIdsAsync(int userId, int? organizationId, CancellationToken ct = default)
    {
        var ids = BuildVisibleNotifications(userId, organizationId)
            .Where(notification => reads.All(x => x.UserId != userId || x.NotificationId != notification.Id))
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => x.Id)
            .ToList();

        return Task.FromResult(ids);
    }

    private IEnumerable<Notification> BuildVisibleNotifications(int userId, int? organizationId)
    {
        return notifications.Where(x =>
            x.UserId == userId
            || (x.UserId == null && organizationId.HasValue && x.OrganizationId == organizationId.Value)
            || (x.UserId == null && x.OrganizationId == null));
    }
}

file sealed class FakeNotificationEmailDispatcher : INotificationEmailDispatcher
{
    public List<(Notification Notification, NotificationDelivery Delivery)> DispatchCalls { get; } = [];

    public Task DispatchAsync(Notification notification, NotificationDelivery delivery, CancellationToken ct = default)
    {
        DispatchCalls.Add((notification, delivery));
        return Task.CompletedTask;
    }

    public Task DispatchPendingAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeNotificationEmailSender : Application.Abstractions.Integration.IEmailSender
{
    public SharedKernel.Results.Result Result { get; init; } = SharedKernel.Results.Result.Success();

    public Task<SharedKernel.Results.Result> SendAsync(
        Application.Abstractions.Integration.EmailMessage message,
        CancellationToken ct = default) =>
        Task.FromResult(Result);
}
