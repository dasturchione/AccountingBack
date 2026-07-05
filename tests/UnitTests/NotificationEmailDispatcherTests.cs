using Application.Abstractions.Integration;
using Application.Abstractions;
using Application.Features.Notifications;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class NotificationEmailDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_ShouldSendEmail_AndMarkDeliveryAsSent()
    {
        var users = new List<User>
        {
            new() { Id = 10, UserName = "aziz", FirstName = "Aziz", LastName = "Karimov", Email = "aziz@example.com", LanguageId = LanguageIdConst.RU }
        };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = 10, Title = "Platezh", Body = "Srok istekaet", CreatedDate = DateTime.Today }
        };
        var deliveries = new List<NotificationDelivery>
        {
            new() { Id = 100, NotificationId = 1, Channel = (short)NotificationChannel.Email, Status = (short)NotificationDeliveryStatus.Pending, CreatedDate = DateTime.Today }
        };
        var sender = new FakeEmailSender { Result = Result.Success() };

        var dispatcher = new NotificationEmailDispatcher(
            new NotificationDispatcherQueryRepository<User>(users),
            new NotificationDispatcherQueryRepository<Notification>(notifications),
            new NotificationDispatcherQueryRepository<NotificationDelivery>(deliveries),
            new NotificationDispatcherCommandRepository<NotificationDelivery>(deliveries),
            sender,
            NullLogger<NotificationEmailDispatcher>.Instance);

        await dispatcher.DispatchAsync(notifications[0], deliveries[0]);

        Assert.Single(sender.Messages);
        Assert.Equal("aziz@example.com", sender.Messages.Single().To.Single());
        Assert.Contains("Zdravstvuyte", sender.Messages.Single().HtmlBody);
        Assert.Equal((short)NotificationDeliveryStatus.Sent, deliveries[0].Status);
        Assert.NotNull(deliveries[0].SentAt);
        Assert.Null(deliveries[0].Error);
    }

    [Fact]
    public async Task DispatchAsync_ShouldMarkDeliveryAsFailed_WhenEmailSenderFails()
    {
        var users = new List<User>
        {
            new() { Id = 10, UserName = "aziz", FirstName = "Aziz", LastName = "Karimov", Email = "aziz@example.com", LanguageId = LanguageIdConst.UZ }
        };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = 10, Title = "To'lov", Body = "Muddat yaqin", CreatedDate = DateTime.Today }
        };
        var deliveries = new List<NotificationDelivery>
        {
            new() { Id = 100, NotificationId = 1, Channel = (short)NotificationChannel.Email, Status = (short)NotificationDeliveryStatus.Pending, CreatedDate = DateTime.Today }
        };
        var sender = new FakeEmailSender
        {
            Result = Result.Failure(Error.Problem("Email.SendFailed", "SMTP timeout"))
        };

        var dispatcher = new NotificationEmailDispatcher(
            new NotificationDispatcherQueryRepository<User>(users),
            new NotificationDispatcherQueryRepository<Notification>(notifications),
            new NotificationDispatcherQueryRepository<NotificationDelivery>(deliveries),
            new NotificationDispatcherCommandRepository<NotificationDelivery>(deliveries),
            sender,
            NullLogger<NotificationEmailDispatcher>.Instance);

        await dispatcher.DispatchAsync(notifications[0], deliveries[0]);

        Assert.Single(sender.Messages);
        Assert.Equal((short)NotificationDeliveryStatus.Failed, deliveries[0].Status);
        Assert.Equal("SMTP timeout", deliveries[0].Error);
        Assert.Null(deliveries[0].SentAt);
    }

    [Fact]
    public async Task DispatchAsync_ShouldFailWithoutSending_ForBroadcastNotification()
    {
        var notification = new Notification
        {
            Id = 1,
            UserId = null,
            OrganizationId = 8,
            Title = "Broadcast",
            Body = "Org",
            CreatedDate = DateTime.Today
        };
        var delivery = new NotificationDelivery
        {
            Id = 100,
            NotificationId = 1,
            Channel = (short)NotificationChannel.Email,
            Status = (short)NotificationDeliveryStatus.Pending,
            CreatedDate = DateTime.Today
        };
        var sender = new FakeEmailSender { Result = Result.Success() };

        var dispatcher = new NotificationEmailDispatcher(
            new NotificationDispatcherQueryRepository<User>([]),
            new NotificationDispatcherQueryRepository<Notification>([notification]),
            new NotificationDispatcherQueryRepository<NotificationDelivery>([delivery]),
            new NotificationDispatcherCommandRepository<NotificationDelivery>([delivery]),
            sender,
            NullLogger<NotificationEmailDispatcher>.Instance);

        await dispatcher.DispatchAsync(notification, delivery);

        Assert.Empty(sender.Messages);
        Assert.Equal((short)NotificationDeliveryStatus.Failed, delivery.Status);
        Assert.Contains("personal notifications", delivery.Error);
    }
}

file sealed class FakeEmailSender : IEmailSender
{
    public List<EmailMessage> Messages { get; } = [];
    public Result Result { get; set; } = Result.Success();

    public Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        Messages.Add(message);
        return Task.FromResult(Result);
    }
}

file sealed class NotificationDispatcherCommandRepository<TEntity>(List<TEntity> items) : ICommandRepository<TEntity> where TEntity : class
{
    public Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        items.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        items.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class NotificationDispatcherQueryRepository<TEntity>(IEnumerable<TEntity> items) : IQueryRepository<TEntity>
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

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();
}
