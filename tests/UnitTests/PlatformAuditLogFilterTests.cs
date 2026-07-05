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

public sealed class PlatformAuditLogFilterTests
{
    [Fact]
    public async Task GetAuditLogsAsync_ShouldMapExtendedPlatformFilters()
    {
        var logs = new List<AuditLog>
        {
            new()
            {
                Id = 2001,
                OrganizationId = 51,
                SchemaName = "public",
                TableName = "sale_doc",
                RecordId = "INV-001",
                Action = "UPDATE",
                ChangedUserId = 801,
                RequestId = "req-sale-001",
                ApplicationName = "platform-console",
                ChangedDate = new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 2002,
                OrganizationId = 52,
                SchemaName = "public",
                TableName = "pur_doc",
                RecordId = "PO-001",
                Action = "UPDATE",
                ChangedUserId = 802,
                RequestId = "req-pur-001",
                ApplicationName = "org-portal",
                ChangedDate = new DateTime(2026, 7, 5, 11, 0, 0, DateTimeKind.Utc)
            }
        };
        var users = new List<User> { CreateUser(801, "global-auditor") };
        var organizations = new List<Organization> { CreateOrganization(51, "ORG-51", "510000001") };
        var service = CreatePlatformService(
            new FakeUserContext { Id = 900, HasGlobalAccess = true },
            logs,
            users,
            organizations);

        var result = await service.GetAuditLogsAsync(new PlatformAuditLogListFilter
        {
            UserId = 801,
            EntityType = "sale_doc",
            EntityId = "INV-001",
            Action = "UPDATE",
            SearchText = "sale-001",
            FromDate = new DateTime(2026, 7, 5, 9, 30, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2026, 7, 5, 10, 30, 0, DateTimeKind.Utc),
            Page = 1,
            PageSize = 20
        });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(2001, result.Value.Items.Single().Id);
        Assert.Equal("ORG-51", result.Value.Items.Single().OrganizationName);
        Assert.Equal("global-auditor", result.Value.Items.Single().ChangedUserName);
    }

    private static PlatformService CreatePlatformService(
        FakeUserContext userContext,
        List<AuditLog> logs,
        List<User> users,
        List<Organization> organizations)
    {
        var queryCore = new AuditLogQueryCore(
            userContext,
            new InMemoryQueryRepository<AuditLog>(logs),
            new InMemoryQueryRepository<User>(users),
            new InMemoryQueryRepository<Organization>(organizations));

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
            new PlatformAuditUserManagementCoreStub(),
            new PlatformAuditOrganizationManagementCoreStub(),
            new InMemoryQueryRepository<Role>([]),
            new InMemoryQueryRepository<TaxType>([]),
            new InMemoryQueryRepository<AccountingPolicy>([]),
            new InMemoryQueryRepository<Currency>([]),
            new InMemoryQueryRepository<OrganizationSetupState>([]),
            queryCore,
            new PlatformAuditOrganizationSetupCoreStub(),
            new PlatformAuditDashboardServiceStub(),
            NullLogger<PlatformService>.Instance,
            new PlatformAuditUnitOfWork());
    }

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

file sealed class PlatformAuditUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class PlatformAuditUserManagementCoreStub : IUserManagementCore
{
    public Task<Result<UserManagementCreateResult>> CreateUserAsync(UserManagementCreateRequest request, UserManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<Result> UpdateUserAsync(UserManagementUpdateRequest request, UserManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task SendWelcomeEmailSafeAsync(UserWelcomeEmailMessage message, CancellationToken ct = default) =>
        Task.CompletedTask;
}

file sealed class PlatformAuditOrganizationManagementCoreStub : IOrganizationManagementCore
{
    public Task<Result<Organization>> GetOrganizationAsync(int organizationId, OrganizationManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<Result> UpdateOrganizationAsync(OrganizationManagementUpdateRequest request, OrganizationManagementOptions options, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

file sealed class PlatformAuditOrganizationSetupCoreStub : IOrganizationSetupCore
{
    public Task UpsertTaxSettingsAsync(int organizationId, OrganizationSetupTaxSettingsWriteModel model, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpsertAccountingPolicyAsync(int organizationId, OrganizationSetupAccountingPolicyWriteModel model, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpsertDefaultsAsync(int organizationId, OrganizationSetupDefaultsWriteModel model, CancellationToken ct = default) => Task.CompletedTask;
    public Task SeedWorkspaceSetupAsync(WorkspaceSetupInitializationRequest request, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateSetupStateAsync(int organizationId, Action<OrganizationSetupState> update, CancellationToken ct = default) => Task.CompletedTask;
    public Task<Result> CompleteSetupAsync(Organization organization, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class PlatformAuditDashboardServiceStub : IDashboardService
{
    public Task<Result<DashboardStatsDto>> GetStatsAsync(CancellationToken ct = default) =>
        Task.FromResult(Result.Success<DashboardStatsDto>(new DashboardStatsDto()));
}
