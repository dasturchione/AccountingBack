using Application.Features.AuditLogs;
using Domain.Entities;

namespace UnitTests;

public sealed class AuditLogQueryCoreTests
{
    [Fact]
    public async Task QueryAsync_RecordScope_ShouldReturnOnlyVisibleRecordLogs()
    {
        var logs = new List<AuditLog>
        {
            CreateAuditLog(101, 1, "snapshot_doc", "42", "UPDATE", 701, new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc)),
            CreateAuditLog(102, 2, "snapshot_doc", "42", "UPDATE", 702, new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc))
        };
        var core = CreateCore(
            new FakeUserContext { OrganizationId = 1, AllowedOrganizationIds = [1] },
            logs,
            [],
            []);

        var result = await core.QueryAsync(
            new AuditLogQueryFilter
            {
                TableName = "snapshot_doc",
                RecordId = "42"
            },
            AuditLogQueryOptions.ForRecord());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(101, result.Value.Single().Id);
        Assert.Null(result.Value.Single().ChangedUserName);
    }

    [Fact]
    public async Task QueryPagedAsync_GlobalScope_ShouldRequireGlobalAccess()
    {
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = false },
            [],
            [],
            []);

        var result = await core.QueryPagedAsync(
            new AuditLogQueryFilter(),
            AuditLogQueryOptions.ForGlobal(),
            new AuditLogQueryPagination());

        Assert.False(result.IsSuccess);
        Assert.Equal("Platform.GlobalAccessRequired", result.Error.Code);
    }

    [Fact]
    public async Task QueryPagedAsync_GlobalScope_ShouldEnrichProjection_AndPaginate()
    {
        var logs = new List<AuditLog>
        {
            CreateAuditLog(1101, 901, "workspace_bootstrap", "100", "CREATE", 901, new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc)),
            CreateAuditLog(1102, 902, "workspace_bootstrap", "100", "UPDATE", 902, new DateTime(2026, 7, 5, 11, 0, 0, DateTimeKind.Utc)),
            CreateAuditLog(1103, 903, "other_table", "999", "UPDATE", 903, new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc))
        };
        var users = new List<User>
        {
            new User
            {
                Id = 901,
                UserName = "global-auditor-1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "998900000901",
                FirstName = "Global",
                LastName = "Auditor 1",
                RoleId = 1,
                StateId = 1,
                CreatedDate = DateTime.UtcNow
            },
            new User
            {
                Id = 902,
                UserName = "global-auditor-2",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "998900000902",
                FirstName = "Global",
                LastName = "Auditor 2",
                RoleId = 1,
                StateId = 1,
                CreatedDate = DateTime.UtcNow
            }
        };
        var organizations = new List<Organization>
        {
            new Organization
            {
                Id = 901,
                ShortName = "AUD-ORG-1",
                FullName = "Audit Organization 1",
                Inn = "901901901",
                RegionId = 1,
                IsParent = true,
                StateId = 1,
                SetupStatus = "pending",
                CreatedDate = DateTime.UtcNow
            },
            new Organization
            {
                Id = 902,
                ShortName = "AUD-ORG-2",
                FullName = "Audit Organization 2",
                Inn = "902902902",
                RegionId = 1,
                IsParent = true,
                StateId = 1,
                SetupStatus = "pending",
                CreatedDate = DateTime.UtcNow
            }
        };
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = true },
            logs,
            users,
            organizations);

        var result = await core.QueryPagedAsync(
            new AuditLogQueryFilter
            {
                TableName = "workspace_bootstrap",
                RecordId = "100"
            },
            AuditLogQueryOptions.ForGlobal(),
            new AuditLogQueryPagination
            {
                Page = 1,
                PageSize = 10
            });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(new long[] { 1102L, 1101L }, result.Value.Items.Select(item => item.Id).ToArray());
        Assert.Equal("AUD-ORG-2", result.Value.Items.First().OrganizationName);
        Assert.Equal("global-auditor-2", result.Value.Items.First().ChangedUserName);
    }

    [Fact]
    public async Task QueryPagedAsync_OrganizationScope_ShouldLimitVisibleOrganizations()
    {
        var logs = new List<AuditLog>
        {
            CreateAuditLog(1201, 8, "pur_doc", "100", "UPDATE", 801, new DateTime(2026, 7, 5, 8, 0, 0, DateTimeKind.Utc)),
            CreateAuditLog(1202, 9, "pur_doc", "100", "UPDATE", 901, new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc))
        };
        var core = CreateCore(
            new FakeUserContext { AllowedOrganizationIds = [8] },
            logs,
            [],
            []);

        var result = await core.QueryPagedAsync(
            new AuditLogQueryFilter
            {
                TableName = "pur_doc"
            },
            AuditLogQueryOptions.ForOrganization(),
            new AuditLogQueryPagination
            {
                Page = 1,
                PageSize = 20
            });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1201, result.Value.Items.Single().Id);
    }

    [Fact]
    public async Task QueryPagedAsync_GlobalScope_ShouldSupportExtendedFilters_AndSearch()
    {
        var logs = new List<AuditLog>
        {
            new()
            {
                Id = 1301,
                OrganizationId = 1,
                SchemaName = "public",
                TableName = "sale_doc",
                RecordId = "INV-001",
                Action = "UPDATE",
                ChangedUserId = 71,
                RequestId = "req-sale-001",
                ApplicationName = "platform-console",
                ChangedDate = new DateTime(2026, 7, 5, 11, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 1302,
                OrganizationId = 1,
                SchemaName = "public",
                TableName = "sale_doc",
                RecordId = "INV-002",
                Action = "CREATE",
                ChangedUserId = 71,
                RequestId = "req-sale-002",
                ApplicationName = "platform-console",
                ChangedDate = new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 1303,
                OrganizationId = 2,
                SchemaName = "public",
                TableName = "pur_doc",
                RecordId = "PO-001",
                Action = "UPDATE",
                ChangedUserId = 72,
                RequestId = "req-pur-001",
                ApplicationName = "org-portal",
                ChangedDate = new DateTime(2026, 7, 5, 13, 0, 0, DateTimeKind.Utc)
            }
        };
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = true },
            logs,
            [],
            []);

        var result = await core.QueryPagedAsync(
            new AuditLogQueryFilter
            {
                UserId = 71,
                EntityType = "sale_doc",
                EntityId = "INV-001",
                Action = "UPDATE",
                SearchText = "sale-001",
                FromDate = new DateTime(2026, 7, 5, 10, 30, 0, DateTimeKind.Utc),
                ToDate = new DateTime(2026, 7, 5, 11, 30, 0, DateTimeKind.Utc)
            },
            AuditLogQueryOptions.ForGlobal(),
            new AuditLogQueryPagination
            {
                Page = 1,
                PageSize = 20
            });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1301, result.Value.Items.Single().Id);
    }

    [Fact]
    public async Task QueryPagedAsync_OrganizationScope_FilterShouldNotBypassCrossTenantVisibility()
    {
        var logs = new List<AuditLog>
        {
            CreateAuditLog(1401, 8, "pur_doc", "100", "UPDATE", 801, new DateTime(2026, 7, 5, 8, 0, 0, DateTimeKind.Utc)),
            CreateAuditLog(1402, 9, "pur_doc", "100", "UPDATE", 901, new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc))
        };
        var core = CreateCore(
            new FakeUserContext { AllowedOrganizationIds = [8] },
            logs,
            [],
            []);

        var result = await core.QueryPagedAsync(
            new AuditLogQueryFilter
            {
                OrganizationId = 9,
                UserId = 901,
                EntityType = "pur_doc",
                SearchText = "100"
            },
            AuditLogQueryOptions.ForOrganization(),
            new AuditLogQueryPagination
            {
                Page = 1,
                PageSize = 20
            });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }

    private static AuditLogQueryCore CreateCore(
        FakeUserContext userContext,
        List<AuditLog> logs,
        List<User> users,
        List<Organization> organizations) =>
        new(
            userContext,
            new InMemoryQueryRepository<AuditLog>(logs),
            new InMemoryQueryRepository<User>(users),
            new InMemoryQueryRepository<Organization>(organizations));

    private static AuditLog CreateAuditLog(
        long id,
        int? organizationId,
        string tableName,
        string recordId,
        string action,
        int? changedUserId,
        DateTime changedDate) =>
        new()
        {
            Id = id,
            OrganizationId = organizationId,
            SchemaName = "public",
            TableName = tableName,
            RecordId = recordId,
            Action = action,
            OldData = "{\"status\":\"draft\"}",
            NewData = "{\"status\":\"posted\"}",
            ChangedUserId = changedUserId,
            ChangedDate = changedDate
        };
}
