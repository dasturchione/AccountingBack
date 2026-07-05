using Application.Features.Platform;
using Application.Features.Notifications;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetStatsAsync_GlobalUser_ShouldReturnExpectedAggregates()
    {
        var now = DateTime.Now;
        var service = CreateService(
            new FakeUserContext { HasGlobalAccess = true },
            tenants:
            [
                new PlatformTenant { Id = 1, Name = "T1", Slug = "t1", StateId = StateIdConst.ACTIVE, CreatedDate = now },
                new PlatformTenant { Id = 2, Name = "T2", Slug = "t2", StateId = StateIdConst.PASSIVE, CreatedDate = now }
            ],
            organizations:
            [
                new Organization { Id = 11, ShortName = "O1", FullName = "Org 1", Inn = "111", RegionId = 1, IsParent = true, StateId = StateIdConst.ACTIVE, SetupStatus = "completed", CreatedDate = now },
                new Organization { Id = 12, ShortName = "O2", FullName = "Org 2", Inn = "222", RegionId = 1, IsParent = true, StateId = StateIdConst.PASSIVE, SetupStatus = "pending", CreatedDate = now }
            ],
            users:
            [
                new User { Id = 101, UserName = "active-recent", PasswordHash = "h", PasswordSalt = "s", PhoneNumber = "1", FirstName = "A", LastName = "B", RoleId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = now.AddDays(-5), LastAccessTime = now.AddDays(-1) },
                new User { Id = 102, UserName = "blocked-old", PasswordHash = "h", PasswordSalt = "s", PhoneNumber = "2", FirstName = "C", LastName = "D", RoleId = 1, StateId = StateIdConst.PASSIVE, CreatedDate = now.AddDays(-40), LastAccessTime = now.AddDays(-31) },
                new User { Id = 103, UserName = "active-old", PasswordHash = "h", PasswordSalt = "s", PhoneNumber = "3", FirstName = "E", LastName = "F", RoleId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = now.AddDays(-10), LastAccessTime = null }
            ],
            auditLogs:
            [
                new AuditLog { Id = 1, OrganizationId = 11, SchemaName = "public", TableName = "x", Action = "UPDATE", ChangedDate = now.AddDays(-2) },
                new AuditLog { Id = 2, OrganizationId = 12, SchemaName = "public", TableName = "x", Action = "UPDATE", ChangedDate = now.AddDays(-10) }
            ],
            notifications:
            [
                new Notification { Id = 1, TypeId = 1, Title = "N1", Body = "B", StateId = StateIdConst.ACTIVE, CreatedDate = now.AddDays(-2) },
                new Notification { Id = 2, TypeId = 1, Title = "N2", Body = "B", StateId = StateIdConst.ACTIVE, CreatedDate = now.AddDays(-20) }
            ],
            deliveries:
            [
                new NotificationDelivery { Id = 1, NotificationId = 1, Channel = (short)NotificationChannel.Email, Status = (short)NotificationDeliveryStatus.Sent, CreatedDate = now },
                new NotificationDelivery { Id = 2, NotificationId = 2, Channel = (short)NotificationChannel.Email, Status = (short)NotificationDeliveryStatus.Failed, CreatedDate = now },
                new NotificationDelivery { Id = 3, NotificationId = 2, Channel = (short)NotificationChannel.InApp, Status = (short)NotificationDeliveryStatus.Sent, CreatedDate = now }
            ],
            settings:
            [
                new SystemSetting { Id = 1, Code = "SYSTEM_NAME", Value = "Accounting", ValueType = 0, IsEditable = true, StateId = StateIdConst.ACTIVE, CreatedDate = now },
                new SystemSetting { Id = 2, Code = "ORG_OVERRIDE", Value = "x", ValueType = 0, IsEditable = true, StateId = StateIdConst.ACTIVE, OrganizationId = 11, CreatedDate = now }
            ],
            roles:
            [
                new Role { Id = 1, ShortName = "admin", FullName = "Admin", StateId = StateIdConst.ACTIVE, CreatedDate = now },
                new Role { Id = 2, ShortName = "blocked", FullName = "Blocked", StateId = StateIdConst.PASSIVE, CreatedDate = now }
            ]);

        var result = await service.GetStatsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TenantsCount);
        Assert.Equal(1, result.Value.ActiveTenantsCount);
        Assert.Equal(1, result.Value.InactiveTenantsCount);
        Assert.Equal(2, result.Value.OrganizationsCount);
        Assert.Equal(1, result.Value.ActiveOrganizationsCount);
        Assert.Equal(1, result.Value.InactiveOrganizationsCount);
        Assert.Equal(3, result.Value.UsersCount);
        Assert.Equal(2, result.Value.ActiveUsersCount);
        Assert.Equal(1, result.Value.BlockedUsersCount);
        Assert.Equal(2, result.Value.UserStats.RecentSignupsLast30Days);
        Assert.Equal(1, result.Value.UserStats.RecentlyActiveLast30Days);
        Assert.Equal(1, result.Value.ActivityStats.RecentAuditsLast7Days);
        Assert.Equal(2, result.Value.NotificationStats.Total);
        Assert.Equal(1, result.Value.NotificationStats.Last7Days);
        Assert.Equal(1, result.Value.NotificationStats.EmailSent);
        Assert.Equal(1, result.Value.NotificationStats.EmailFailed);
        Assert.Equal(1, result.Value.SystemStats.SettingsCount);
        Assert.Equal(1, result.Value.SystemStats.RolesCount);
    }

    [Fact]
    public async Task GetStatsAsync_NonGlobalUser_ShouldBeForbidden()
    {
        var service = CreateService(
            new FakeUserContext { HasGlobalAccess = false },
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            []);

        var result = await service.GetStatsAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("Platform.GlobalAccessRequired", result.Error.Code);
    }

    private static DashboardService CreateService(
        FakeUserContext userContext,
        List<PlatformTenant> tenants,
        List<Organization> organizations,
        List<User> users,
        List<AuditLog> auditLogs,
        List<Notification> notifications,
        List<NotificationDelivery> deliveries,
        List<SystemSetting> settings,
        List<Role> roles) =>
        new(
            userContext,
            new InMemoryQueryRepository<PlatformTenant>(tenants),
            new InMemoryQueryRepository<Organization>(organizations),
            new InMemoryQueryRepository<User>(users),
            new InMemoryQueryRepository<AuditLog>(auditLogs),
            new InMemoryQueryRepository<Notification>(notifications),
            new InMemoryQueryRepository<NotificationDelivery>(deliveries),
            new InMemoryQueryRepository<SystemSetting>(settings),
            new InMemoryQueryRepository<Role>(roles),
            NullLogger<DashboardService>.Instance,
            new DashboardFakeUnitOfWork());
}

file sealed class DashboardFakeUnitOfWork : Application.Abstractions.IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}
