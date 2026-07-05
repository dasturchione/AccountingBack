using Application.Abstractions;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.Organizations;
using Application.Features.OrganizationSetup;
using Application.Features.Platform;
using Application.Features.Users.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace UnitTests;

public sealed class SuperAdminAuditSafetyNetTests
{
    [Fact]
    public async Task AuditLogService_RecordView_ShouldReturnChangeResultsSnapshot()
    {
        var logs = new List<AuditLog>
        {
            new()
            {
                Id = 1001,
                OrganizationId = 1,
                SchemaName = "public",
                TableName = "snapshot_doc",
                RecordId = "42",
                Action = "UPDATE",
                OldData = "{\"status\":\"draft\",\"amount\":10}",
                NewData = "{\"status\":\"posted\",\"amount\":12}",
                ChangedUserId = 101,
                ChangedDate = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc)
            }
        };

        var service = CreateAuditLogService(
            new FakeUserContext { Id = 101, OrganizationId = 1, AllowedOrganizationIds = [1] },
            logs,
            [],
            []);

        var result = await service.GetByRecordAsync(new AuditLogFilter
        {
            TableName = "snapshot_doc",
            RecordId = "42"
        });

        Assert.Single(result);
        Assert.Contains(result.Single().ChangeResults, x => x.Path == "status" && Equals(x.OldValue?.ToString(), "draft") && Equals(x.NewValue?.ToString(), "posted"));
        Assert.Contains(result.Single().ChangeResults, x => x.Path == "amount" && Equals(x.OldValue?.ToString(), "10") && Equals(x.NewValue?.ToString(), "12"));
    }

    [Fact]
    public async Task AuditLogService_RecordView_GlobalUser_ShouldSeeAllOrganizations()
    {
        var service = CreateAuditLogService(
            new FakeUserContext { Id = 900, HasGlobalAccess = true },
            CreatePurDocLogs(),
            [],
            []);

        var result = await service.GetByRecordAsync(new AuditLogFilter
        {
            TableName = "pur_doc",
            RecordId = "100"
        });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AuditLogService_RecordView_OrganizationUser_ShouldSeeOnlyOwnOrganization()
    {
        var service = CreateAuditLogService(
            new FakeUserContext { Id = 101, OrganizationId = 1, AllowedOrganizationIds = [1] },
            CreatePurDocLogs(),
            [],
            []);

        var result = await service.GetByRecordAsync(new AuditLogFilter
        {
            TableName = "pur_doc",
            RecordId = "100"
        });

        Assert.Single(result);
        Assert.All(result, log => Assert.Equal(1, log.OrganizationId));
    }

    [Fact]
    public async Task AuditLogService_RecordView_UserWithoutOrganizations_ShouldReceiveNoData()
    {
        var service = CreateAuditLogService(
            new FakeUserContext { Id = 777 },
            CreatePurDocLogs(),
            [],
            []);

        var result = await service.GetByRecordAsync(new AuditLogFilter
        {
            TableName = "pur_doc",
            RecordId = "100"
        });

        Assert.Empty(result);
    }

    [Fact]
    public async Task AuditLogQueryCore_OrganizationScope_ShouldProtectAgainstCrossTenantReads()
    {
        var core = CreateAuditLogQueryCore(
            new FakeUserContext { Id = 101, AllowedOrganizationIds = [1] },
            CreatePurDocLogs(),
            [],
            []);

        var result = await core.QueryPagedAsync(
            new AuditLogQueryFilter
            {
                TableName = "pur_doc",
                RecordId = "100"
            },
            AuditLogQueryOptions.ForOrganization(),
            new AuditLogQueryPagination
            {
                Page = 1,
                PageSize = 20
            });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.All(result.Value.Items, log => Assert.Equal(1, log.OrganizationId));
    }

    [Fact]
    public async Task PlatformAuditLogs_GlobalUser_ShouldReturnCurrentGlobalProjection()
    {
        var logs = new List<AuditLog>
        {
            new()
            {
                Id = 1101,
                OrganizationId = 901,
                SchemaName = "public",
                TableName = "workspace_bootstrap",
                RecordId = "100",
                Action = "CREATE",
                NewData = "{\"status\":\"pending\"}",
                ChangedUserId = 901,
                ChangedDate = new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 1102,
                OrganizationId = 902,
                SchemaName = "public",
                TableName = "workspace_bootstrap",
                RecordId = "100",
                Action = "UPDATE",
                OldData = "{\"status\":\"pending\"}",
                NewData = "{\"status\":\"completed\"}",
                ChangedUserId = 902,
                ChangedDate = new DateTime(2026, 7, 5, 11, 0, 0, DateTimeKind.Utc)
            }
        };
        var users = new List<User>
        {
            CreateUser(901, "global-auditor-1"),
            CreateUser(902, "global-auditor-2")
        };
        var organizations = new List<Organization>
        {
            CreateOrganization(901, "AUD-ORG-1", "901901901"),
            CreateOrganization(902, "AUD-ORG-2", "902902902")
        };
        var service = CreatePlatformService(
            new FakeUserContext { Id = 900, HasGlobalAccess = true },
            logs,
            users,
            organizations);

        var result = await service.GetAuditLogsAsync(new PlatformAuditLogListFilter
        {
            TableName = "workspace_bootstrap",
            RecordId = "100"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(new long[] { 1102, 1101 }, result.Value.Items.Select(x => x.Id).ToArray());
        Assert.Equal("AUD-ORG-2", result.Value.Items.First().OrganizationName);
        Assert.Equal("global-auditor-2", result.Value.Items.First().ChangedUserName);
        Assert.Equal("AUD-ORG-1", result.Value.Items.Last().OrganizationName);
        Assert.Equal("global-auditor-1", result.Value.Items.Last().ChangedUserName);
    }

    [Fact]
    public async Task PlatformAuditLogs_NonGlobalUser_ShouldBeForbidden()
    {
        var service = CreatePlatformService(
            new FakeUserContext { Id = 101, OrganizationId = 1, AllowedOrganizationIds = [1] },
            [],
            [],
            []);

        var result = await service.GetAuditLogsAsync(new PlatformAuditLogListFilter());

        Assert.False(result.IsSuccess);
        Assert.Equal("Platform.GlobalAccessRequired", result.Error.Code);
    }

    private static AuditLogService CreateAuditLogService(
        FakeUserContext userContext,
        List<AuditLog> logs,
        List<User> users,
        List<Organization> organizations)
    {
        var queryCore = CreateAuditLogQueryCore(userContext, logs, users, organizations);
        return new AuditLogService(
            userContext,
            new InMemoryCommandRepository<AuditLog>(logs),
            queryCore);
    }

    private static AuditLogQueryCore CreateAuditLogQueryCore(
        FakeUserContext userContext,
        List<AuditLog> logs,
        List<User> users,
        List<Organization> organizations) =>
        new(
            userContext,
            new InMemoryQueryRepository<AuditLog>(logs),
            new InMemoryQueryRepository<User>(users),
            new InMemoryQueryRepository<Organization>(organizations));

    private static PlatformService CreatePlatformService(
        FakeUserContext userContext,
        List<AuditLog> logs,
        List<User> users,
        List<Organization> organizations)
    {
        var queryCore = CreateAuditLogQueryCore(userContext, logs, users, organizations);

        return new PlatformService(
            userContext,
            new FakePasswordHasher(),
            new InMemoryQueryRepository<PlatformTenant>([]),
            new InMemoryCommandRepository<PlatformTenant>([]),
            new InMemoryQueryRepository<Organization>(organizations),
            new InMemoryCommandRepository<Organization>(organizations),
            new InMemoryQueryRepository<User>(users),
            new InMemoryCommandRepository<User>(users),
            new InMemoryQueryRepository<UserOrganization>([]),
            new InMemoryCommandRepository<UserOrganization>([]),
            new AuditUserManagementCoreStub(),
            new AuditOrganizationManagementCoreStub(),
            new InMemoryQueryRepository<Role>([]),
            new InMemoryQueryRepository<TaxType>([]),
            new InMemoryQueryRepository<AccountingPolicy>([]),
            new InMemoryQueryRepository<Currency>([]),
            new InMemoryQueryRepository<OrganizationSetupState>([]),
            queryCore,
            new AuditOrganizationSetupCoreStub(),
            new AuditDashboardServiceStub(),
            NullLogger<PlatformService>.Instance,
            new AuditSafetyNetUnitOfWork());
    }

    private static List<AuditLog> CreatePurDocLogs() =>
        [
            new AuditLog
            {
                Id = 1,
                OrganizationId = 1,
                SchemaName = "public",
                TableName = "pur_doc",
                RecordId = "100",
                Action = "UPDATE",
                OldData = "{\"status\":\"draft\"}",
                NewData = "{\"status\":\"posted\"}",
                ChangedUserId = 10,
                ChangedDate = new DateTime(2026, 7, 5, 8, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                Id = 2,
                OrganizationId = 2,
                SchemaName = "public",
                TableName = "pur_doc",
                RecordId = "100",
                Action = "UPDATE",
                OldData = "{\"status\":\"draft\"}",
                NewData = "{\"status\":\"posted\"}",
                ChangedUserId = 20,
                ChangedDate = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc)
            }
        ];

    private static User CreateUser(int id, string userName) =>
        new()
        {
            Id = id,
            UserName = userName,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            PhoneNumber = $"998900000{id}",
            FirstName = "Global",
            LastName = "Auditor",
            RoleId = 1,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.UtcNow
        };

    private static Organization CreateOrganization(int id, string shortName, string inn) =>
        new()
        {
            Id = id,
            ShortName = shortName,
            FullName = shortName,
            Inn = inn,
            RegionId = 1,
            IsParent = true,
            StateId = StateIdConst.ACTIVE,
            SetupStatus = "pending",
            CreatedDate = DateTime.UtcNow
        };
}

file sealed class AuditSafetyNetUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class AuditUserManagementCoreStub : IUserManagementCore
{
    public Task<Result<UserManagementCreateResult>> CreateUserAsync(UserManagementCreateRequest request, UserManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<Result> UpdateUserAsync(UserManagementUpdateRequest request, UserManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task SendWelcomeEmailSafeAsync(UserWelcomeEmailMessage message, CancellationToken ct = default) =>
        Task.CompletedTask;
}

file sealed class AuditOrganizationManagementCoreStub : IOrganizationManagementCore
{
    public Task<Result<Organization>> GetOrganizationAsync(int organizationId, OrganizationManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<Result> UpdateOrganizationAsync(OrganizationManagementUpdateRequest request, OrganizationManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

file sealed class AuditOrganizationSetupCoreStub : IOrganizationSetupCore
{
    public Task UpsertTaxSettingsAsync(int organizationId, OrganizationSetupTaxSettingsWriteModel model, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpsertAccountingPolicyAsync(int organizationId, OrganizationSetupAccountingPolicyWriteModel model, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpsertDefaultsAsync(int organizationId, OrganizationSetupDefaultsWriteModel model, CancellationToken ct = default) => Task.CompletedTask;
    public Task SeedWorkspaceSetupAsync(WorkspaceSetupInitializationRequest request, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateSetupStateAsync(int organizationId, Action<OrganizationSetupState> update, CancellationToken ct = default) => Task.CompletedTask;
    public Task<Result> CompleteSetupAsync(Organization organization, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class AuditDashboardServiceStub : IDashboardService
{
    public Task<Result<DashboardStatsDto>> GetStatsAsync(CancellationToken ct = default) =>
        Task.FromResult(Result.Success<DashboardStatsDto>(new DashboardStatsDto()));
}
