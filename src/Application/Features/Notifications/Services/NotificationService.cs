using System.Linq.Expressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Results;
using SharedKernel.Query.Specifications;

namespace Application.Features.Notifications;

public sealed class NotificationService : BaseService, INotificationService
{
    private readonly IUserContext _userContext;
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly IQueryRepository<Notification> _notificationQuery;
    private readonly ICommandRepository<Notification> _notificationCommand;
    private readonly ICommandRepository<NotificationDelivery> _notificationDeliveryCommand;
    private readonly IQueryRepository<NotificationRead> _notificationReadQuery;
    private readonly ICommandRepository<NotificationRead> _notificationReadCommand;
    private readonly IQueryRepository<NotificationType> _notificationTypeQuery;
    private readonly INotificationEmailDispatcher _notificationEmailDispatcher;

    public NotificationService(
        IUserContext userContext,
        INotificationReadRepository notificationReadRepository,
        IQueryRepository<Notification> notificationQuery,
        ICommandRepository<Notification> notificationCommand,
        ICommandRepository<NotificationDelivery> notificationDeliveryCommand,
        IQueryRepository<NotificationRead> notificationReadQuery,
        ICommandRepository<NotificationRead> notificationReadCommand,
        IQueryRepository<NotificationType> notificationTypeQuery,
        INotificationEmailDispatcher notificationEmailDispatcher,
        ILogger<NotificationService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _notificationReadRepository = notificationReadRepository;
        _notificationQuery = notificationQuery;
        _notificationCommand = notificationCommand;
        _notificationDeliveryCommand = notificationDeliveryCommand;
        _notificationReadQuery = notificationReadQuery;
        _notificationReadCommand = notificationReadCommand;
        _notificationTypeQuery = notificationTypeQuery;
        _notificationEmailDispatcher = notificationEmailDispatcher;
    }

    public async Task<Result<long>> CreateAsync(CreateNotificationRequest request, CancellationToken ct = default)
    {
        Notification? createdNotification = null;
        NotificationDelivery? emailDelivery = null;

        var result = await ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result.Failure<long>(NotificationErrors.TitleRequired(_userContext.LanguageId));

            if (string.IsNullOrWhiteSpace(request.Body))
                return Result.Failure<long>(NotificationErrors.BodyRequired(_userContext.LanguageId));

            var typeResult = await ResolveNotificationTypeAsync(request, ct);
            if (!typeResult.IsSuccess)
                return Result.Failure<long>(typeResult.Error);

            var channelsResult = NormalizeChannels(request.Channels);
            if (!channelsResult.IsSuccess)
                return Result.Failure<long>(channelsResult.Error);

            createdNotification = new Notification
            {
                OrganizationId = request.OrganizationId,
                UserId = request.UserId,
                TypeId = typeResult.Value.Id,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                Link = NormalizeOptional(request.Link),
                EntityType = NormalizeOptional(request.EntityType),
                EntityId = request.EntityId,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            await _notificationCommand.CreateAsync(createdNotification, ct);

            var now = DateTime.Now;
            var deliveries = channelsResult.Value.Select(channel => new NotificationDelivery
            {
                NotificationId = createdNotification.Id,
                Channel = (short)channel,
                Status = channel == NotificationChannel.InApp
                    ? (short)NotificationDeliveryStatus.Sent
                    : (short)NotificationDeliveryStatus.Pending,
                SentAt = channel == NotificationChannel.InApp ? now : null,
                CreatedDate = now
            }).ToList();

            await _notificationDeliveryCommand.CreateAsync(deliveries, ct);
            emailDelivery = deliveries.FirstOrDefault(x => x.Channel == (short)NotificationChannel.Email);

            return createdNotification.Id;
        }, ct);

        if (result.IsSuccess && createdNotification is not null && emailDelivery is not null)
            await _notificationEmailDispatcher.DispatchAsync(createdNotification, emailDelivery, ct);

        return result;
    }

    public Task<Result<NotificationListResponse>> GetForUserAsync(NotificationQuery query, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetForUserAsync), async () =>
        {
            if (_userContext.Id is null)
                return Result.Failure<NotificationListResponse>(CommonErrors.Unauthorized(_userContext.LanguageId));

            var userId = _userContext.Id.Value;
            var orgId = _userContext.OrganizationId;
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize is > 0 ? query.PageSize.Value : 20;

            var paged = await _notificationReadRepository.GetForUserAsync(userId, orgId, query, ct);
            var unreadCount = await _notificationReadRepository.GetUnreadCountAsync(userId, orgId, ct);
            var typeLookup = await LoadNotificationTypeLookupAsync(paged.Items.Select(x => x.TypeId).Distinct().ToList(), ct);

            var items = paged.Items
                .Select(x =>
                {
                    typeLookup.TryGetValue(x.TypeId, out var type);

                    return new NotificationDto
                    {
                        Id = x.Id,
                        TypeCode = type?.Code ?? string.Empty,
                        TypeName = type?.Name ?? string.Empty,
                        Title = x.Title,
                        Body = x.Body,
                        IsRead = x.IsRead,
                        ReadAt = x.ReadAt,
                        Link = x.Link,
                        CreatedDate = x.CreatedDate
                    };
                })
                .ToList();

            return Result.Success(new NotificationListResponse
            {
                Items = items,
                TotalCount = paged.TotalCount,
                UnreadCount = unreadCount,
                Page = page,
                PageSize = pageSize
            });
        });

    public Task<Result<int>> GetUnreadCountAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetUnreadCountAsync), async () =>
        {
            if (_userContext.Id is null)
                return Result.Failure<int>(CommonErrors.Unauthorized(_userContext.LanguageId));

            var unreadCount = await _notificationReadRepository.GetUnreadCountAsync(_userContext.Id.Value, _userContext.OrganizationId, ct);
            return Result.Success(unreadCount);
        });

    public Task<Result> MarkAsReadAsync(long notificationId, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(MarkAsReadAsync), async () =>
        {
            if (_userContext.Id is null)
                return Result.Failure(CommonErrors.Unauthorized(_userContext.LanguageId));

            var userId = _userContext.Id.Value;
            var isVisible = await _notificationReadRepository.IsVisibleAsync(notificationId, userId, _userContext.OrganizationId, ct);
            if (!isVisible)
                return Result.Failure(NotificationErrors.NotFound(notificationId, _userContext.LanguageId));

            var alreadyRead = await _notificationReadQuery.AnyAsync(x => x.NotificationId == notificationId && x.UserId == userId, ct);
            if (alreadyRead)
                return Result.Success();

            await _notificationReadCommand.CreateAsync(new NotificationRead
            {
                NotificationId = notificationId,
                UserId = userId,
                ReadAt = DateTime.Now,
                CreatedDate = DateTime.Now
            }, ct);
            return Result.Success();
        }, ct);

    public Task<Result> MarkAllAsReadAsync(CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(MarkAllAsReadAsync), async () =>
        {
            if (_userContext.Id is null)
                return Result.Failure(CommonErrors.Unauthorized(_userContext.LanguageId));

            var userId = _userContext.Id.Value;
            var unreadNotificationIds = await _notificationReadRepository.GetUnreadNotificationIdsAsync(userId, _userContext.OrganizationId, ct);
            if (unreadNotificationIds.Count == 0)
                return Result.Success();

            var now = DateTime.Now;
            var reads = unreadNotificationIds.Select(notificationId => new NotificationRead
            {
                NotificationId = notificationId,
                UserId = userId,
                ReadAt = now,
                CreatedDate = now
            });

            await _notificationReadCommand.CreateAsync(reads, ct);
            return Result.Success();
        }, ct);

    private async Task<Result<NotificationType>> ResolveNotificationTypeAsync(CreateNotificationRequest request, CancellationToken ct)
    {
        if (!request.TypeId.HasValue && string.IsNullOrWhiteSpace(request.TypeCode))
            return Result.Failure<NotificationType>(NotificationErrors.TypeRequired(_userContext.LanguageId));

        NotificationType? type = null;
        var normalizedCode = request.TypeCode?.Trim().ToLowerInvariant();

        if (request.TypeId.HasValue)
        {
            type = await _notificationTypeQuery.GetAsync(new QuerySpecification<NotificationType>
            {
                Criteria = x => x.Id == request.TypeId.Value && x.StateId == StateIdConst.ACTIVE
            }, ct);

            if (type is null)
                return Result.Failure<NotificationType>(NotificationErrors.TypeNotFound(request.TypeId.Value, _userContext.LanguageId));

            if (!string.IsNullOrWhiteSpace(normalizedCode) &&
                !string.Equals(type.Code, normalizedCode, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<NotificationType>(NotificationErrors.TypeMismatch(request.TypeId.Value, request.TypeCode!, _userContext.LanguageId));
            }
        }

        if (type is null && !string.IsNullOrWhiteSpace(normalizedCode))
        {
            type = await _notificationTypeQuery.GetAsync(new QuerySpecification<NotificationType>
            {
                Criteria = x => x.Code.ToLower() == normalizedCode && x.StateId == StateIdConst.ACTIVE
            }, ct);

            if (type is null)
                return Result.Failure<NotificationType>(NotificationErrors.TypeNotFound(request.TypeCode!, _userContext.LanguageId));
        }

        return type!;
    }

    private async Task<Dictionary<short, NotificationType>> LoadNotificationTypeLookupAsync(IReadOnlyCollection<short> typeIds, CancellationToken ct)
    {
        if (typeIds.Count == 0)
            return [];

        var types = await _notificationTypeQuery.GetAllAsync(new QuerySpecification<NotificationType>
        {
            Criteria = x => typeIds.Contains(x.Id)
        }, ct);

        return types
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.First());
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Result<IReadOnlyCollection<NotificationChannel>> NormalizeChannels(IReadOnlyCollection<NotificationChannel>? channels)
    {
        var normalized = (channels is null || channels.Count == 0
                ? [NotificationChannel.InApp]
                : channels.ToList())
            .Distinct()
            .ToList();

        if (!normalized.Contains(NotificationChannel.InApp))
            normalized.Insert(0, NotificationChannel.InApp);

        var unsupportedChannel = normalized
            .FirstOrDefault(x => x is not NotificationChannel.InApp and not NotificationChannel.Email);

        if (normalized.Any(x => x is not NotificationChannel.InApp and not NotificationChannel.Email))
        {
            return Result.Failure<IReadOnlyCollection<NotificationChannel>>(
                NotificationErrors.UnsupportedChannel((short)unsupportedChannel, _userContext.LanguageId));
        }

        return Result.Success<IReadOnlyCollection<NotificationChannel>>(normalized);
    }
}
