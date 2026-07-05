using System.Linq.Expressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features;
using Application.Features.Notifications;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Platform;

public sealed class DashboardService : BaseService, IDashboardService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<PlatformTenant> _tenantQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<AuditLog> _auditLogQuery;
    private readonly IQueryRepository<Notification> _notificationQuery;
    private readonly IQueryRepository<NotificationDelivery> _notificationDeliveryQuery;
    private readonly IQueryRepository<SystemSetting> _settingQuery;
    private readonly IQueryRepository<Role> _roleQuery;

    public DashboardService(
        IUserContext userContext,
        IQueryRepository<PlatformTenant> tenantQuery,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<User> userQuery,
        IQueryRepository<AuditLog> auditLogQuery,
        IQueryRepository<Notification> notificationQuery,
        IQueryRepository<NotificationDelivery> notificationDeliveryQuery,
        IQueryRepository<SystemSetting> settingQuery,
        IQueryRepository<Role> roleQuery,
        ILogger<DashboardService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _tenantQuery = tenantQuery;
        _organizationQuery = organizationQuery;
        _userQuery = userQuery;
        _auditLogQuery = auditLogQuery;
        _notificationQuery = notificationQuery;
        _notificationDeliveryQuery = notificationDeliveryQuery;
        _settingQuery = settingQuery;
        _roleQuery = roleQuery;
    }

    public Task<Result<DashboardStatsDto>> GetStatsAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetStatsAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<DashboardStatsDto>(PlatformErrors.GlobalAccessRequired());

            var now = DateTime.Now;
            var recentUserThreshold = now.AddDays(-30);
            var recentNotificationThreshold = now.AddDays(-7);
            var recentAuditThreshold = now.AddDays(-7);

            var tenantsCount = await CountTenantsAsync(_ => true, ct);
            var activeTenantsCount = await CountTenantsAsync(x => x.StateId == StateIdConst.ACTIVE, ct);
            var organizationsCount = await CountOrganizationsAsync(_ => true, ct);
            var activeOrganizationsCount = await CountOrganizationsAsync(x => x.StateId == StateIdConst.ACTIVE, ct);
            var usersCount = await CountUsersAsync(_ => true, ct);
            var activeUsersCount = await CountUsersAsync(x => x.StateId == StateIdConst.ACTIVE, ct);
            var blockedUsersCount = await CountUsersAsync(x => x.StateId == StateIdConst.PASSIVE, ct);
            var recentSignupsCount = await CountUsersAsync(x => x.CreatedDate >= recentUserThreshold, ct);
            var recentlyActiveUsersCount = await CountUsersAsync(
                x => x.LastAccessTime.HasValue && x.LastAccessTime.Value >= recentUserThreshold,
                ct);
            var recentAuditsCount = await CountAuditLogsAsync(x => x.ChangedDate >= recentAuditThreshold, ct);
            var notificationsCount = await CountNotificationsAsync(_ => true, ct);
            var notificationsLast7DaysCount = await CountNotificationsAsync(x => x.CreatedDate >= recentNotificationThreshold, ct);
            var emailSentCount = await CountNotificationDeliveriesAsync(
                x => x.Channel == (short)NotificationChannel.Email && x.Status == (short)NotificationDeliveryStatus.Sent,
                ct);
            var emailFailedCount = await CountNotificationDeliveriesAsync(
                x => x.Channel == (short)NotificationChannel.Email && x.Status == (short)NotificationDeliveryStatus.Failed,
                ct);
            var settingsCount = await CountSettingsAsync(
                x => x.StateId == StateIdConst.ACTIVE && x.OrganizationId == null,
                ct);
            var rolesCount = await CountRolesAsync(x => x.StateId == StateIdConst.ACTIVE, ct);

            return Result.Success(new DashboardStatsDto
            {
                TenantsCount = tenantsCount,
                ActiveTenantsCount = activeTenantsCount,
                InactiveTenantsCount = tenantsCount - activeTenantsCount,
                OrganizationsCount = organizationsCount,
                ActiveOrganizationsCount = activeOrganizationsCount,
                InactiveOrganizationsCount = organizationsCount - activeOrganizationsCount,
                UsersCount = usersCount,
                ActiveUsersCount = activeUsersCount,
                BlockedUsersCount = blockedUsersCount,
                TenantStats = new DashboardTenantStatsDto
                {
                    Total = tenantsCount,
                    Active = activeTenantsCount,
                    Inactive = tenantsCount - activeTenantsCount
                },
                UserStats = new DashboardUserStatsDto
                {
                    Total = usersCount,
                    Active = activeUsersCount,
                    Blocked = blockedUsersCount,
                    RecentSignupsLast30Days = recentSignupsCount,
                    RecentlyActiveLast30Days = recentlyActiveUsersCount
                },
                OrganizationStats = new DashboardOrganizationStatsDto
                {
                    Total = organizationsCount,
                    Active = activeOrganizationsCount,
                    Inactive = organizationsCount - activeOrganizationsCount
                },
                ActivityStats = new DashboardActivityStatsDto
                {
                    RecentAuditsLast7Days = recentAuditsCount
                },
                NotificationStats = new DashboardNotificationStatsDto
                {
                    Total = notificationsCount,
                    Last7Days = notificationsLast7DaysCount,
                    EmailSent = emailSentCount,
                    EmailFailed = emailFailedCount
                },
                SystemStats = new DashboardSystemStatsDto
                {
                    SettingsCount = settingsCount,
                    RolesCount = rolesCount
                }
            });
        });

    private async Task<int> CountTenantsAsync(Expression<Func<PlatformTenant, bool>> criteria, CancellationToken ct)
    {
        var page = await _tenantQuery.GetPagedAsync(new PagedQuerySpecification<PlatformTenant> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountOrganizationsAsync(Expression<Func<Organization, bool>> criteria, CancellationToken ct)
    {
        var page = await _organizationQuery.GetPagedAsync(new PagedQuerySpecification<Organization> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountUsersAsync(Expression<Func<User, bool>> criteria, CancellationToken ct)
    {
        var page = await _userQuery.GetPagedAsync(new PagedQuerySpecification<User> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountAuditLogsAsync(Expression<Func<AuditLog, bool>> criteria, CancellationToken ct)
    {
        var page = await _auditLogQuery.GetPagedAsync(new PagedQuerySpecification<AuditLog> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountNotificationsAsync(Expression<Func<Notification, bool>> criteria, CancellationToken ct)
    {
        var page = await _notificationQuery.GetPagedAsync(new PagedQuerySpecification<Notification> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountNotificationDeliveriesAsync(Expression<Func<NotificationDelivery, bool>> criteria, CancellationToken ct)
    {
        var page = await _notificationDeliveryQuery.GetPagedAsync(new PagedQuerySpecification<NotificationDelivery> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountSettingsAsync(Expression<Func<SystemSetting, bool>> criteria, CancellationToken ct)
    {
        var page = await _settingQuery.GetPagedAsync(new PagedQuerySpecification<SystemSetting> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountRolesAsync(Expression<Func<Role, bool>> criteria, CancellationToken ct)
    {
        var page = await _roleQuery.GetPagedAsync(new PagedQuerySpecification<Role> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }
}
