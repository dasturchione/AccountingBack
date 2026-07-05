using System.Net;
using System.Net.Http.Json;
using Application.Features.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Results;

namespace IntegrationTests;

public class NotificationsControllerIntegrationTests
{
    [Fact]
    public async Task GetNotifications_ShouldReturnServicePayload_AndBindQuery()
    {
        var service = new FakeNotificationService
        {
            GetForUserResponse = Result.Success(new NotificationListResponse
            {
                Items =
                [
                    new NotificationDto
                    {
                        Id = 42,
                        Title = "Hello",
                        Body = "World",
                        TypeCode = "info",
                        TypeName = "Info",
                        IsRead = false,
                        CreatedDate = new DateTime(2026, 7, 5, 10, 0, 0)
                    }
                ],
                TotalCount = 1,
                UnreadCount = 1,
                Page = 2,
                PageSize = 5
            })
        };

        await using var factory = CreateFactory(service);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/notifications?page=2&pageSize=5&isRead=false&typeId=3");
        var payload = await response.Content.ReadFromJsonAsync<NotificationListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(42, payload.Items.Single().Id);
        Assert.NotNull(service.LastQuery);
        Assert.Equal(2, service.LastQuery.Page);
        Assert.Equal(5, service.LastQuery.PageSize);
        Assert.False(service.LastQuery.IsRead);
        Assert.Equal<short>(3, service.LastQuery.TypeId!.Value);
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnBadRequest_ForInvalidQuery()
    {
        var service = new FakeNotificationService();

        await using var factory = CreateFactory(service);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/notifications?page=0&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.LastQuery);
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnBadgePayload()
    {
        var service = new FakeNotificationService
        {
            GetUnreadCountResponse = Result.Success(7)
        };

        await using var factory = CreateFactory(service);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/notifications/unread-count");
        var payload = await response.Content.ReadFromJsonAsync<UnreadCountResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(7, payload.count);
    }

    [Fact]
    public async Task MarkAsRead_ShouldMapNotFound_FromService()
    {
        var service = new FakeNotificationService
        {
            MarkAsReadResponse = Result.Failure(NotificationErrors.NotFound(99, null))
        };

        await using var factory = CreateFactory(service);
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/notifications/99/read", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(99, service.LastMarkedNotificationId);
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnNoContent()
    {
        var service = new FakeNotificationService
        {
            MarkAllAsReadResponse = Result.Success()
        };

        await using var factory = CreateFactory(service);
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/notifications/read-all", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(service.MarkAllAsReadCalled);
    }

    private static TestWebApplicationFactory CreateFactory(FakeNotificationService service) =>
        new(overrideServices: services =>
        {
            services.RemoveAll(typeof(INotificationService));
            services.AddSingleton<INotificationService>(service);
        });

    private sealed class FakeNotificationService : INotificationService
    {
        public Result<long> CreateResponse { get; set; } = Result.Success(1L);
        public Result<NotificationListResponse> GetForUserResponse { get; set; } = Result.Success(new NotificationListResponse());
        public Result<int> GetUnreadCountResponse { get; set; } = Result.Success(0);
        public Result MarkAsReadResponse { get; set; } = Result.Success();
        public Result MarkAllAsReadResponse { get; set; } = Result.Success();
        public NotificationQuery? LastQuery { get; private set; }
        public long? LastMarkedNotificationId { get; private set; }
        public bool MarkAllAsReadCalled { get; private set; }

        public Task<Result<long>> CreateAsync(CreateNotificationRequest request, CancellationToken ct = default) =>
            Task.FromResult(CreateResponse);

        public Task<Result<NotificationListResponse>> GetForUserAsync(NotificationQuery query, CancellationToken ct = default)
        {
            LastQuery = query;
            return Task.FromResult(GetForUserResponse);
        }

        public Task<Result<int>> GetUnreadCountAsync(CancellationToken ct = default) =>
            Task.FromResult(GetUnreadCountResponse);

        public Task<Result> MarkAsReadAsync(long notificationId, CancellationToken ct = default)
        {
            LastMarkedNotificationId = notificationId;
            return Task.FromResult(MarkAsReadResponse);
        }

        public Task<Result> MarkAllAsReadAsync(CancellationToken ct = default)
        {
            MarkAllAsReadCalled = true;
            return Task.FromResult(MarkAllAsReadResponse);
        }
    }

    private sealed record UnreadCountResponse(int count);
}
