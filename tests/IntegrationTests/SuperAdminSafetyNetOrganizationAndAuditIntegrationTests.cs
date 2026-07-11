using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Integration;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.OrganizationSetup;
using Application.Features.Platform;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Constants;

namespace IntegrationTests;

public sealed class SuperAdminSafetyNetOrganizationAndAuditIntegrationTests
{
    [Fact]
    public async Task OrganizationUpdate_ShouldMapFields_AndPreserveSetupStatusWhenBlank()
    {
        await using var factory = CreateFactory();

        await SeedAsync(factory, async db =>
        {
            db.States.Add(new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "ACTIVE",
                FullName = "Active",
                CreatedDate = DateTime.UtcNow
            });

            db.Regions.AddRange(
                new Region
                {
                    Id = 1,
                    ShortName = "R1",
                    FullName = "Region 1",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                },
                new Region
                {
                    Id = 2,
                    ShortName = "R2",
                    FullName = "Region 2",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.Districts.AddRange(
                new District
                {
                    Id = 10,
                    ShortName = "D10",
                    FullName = "District 10",
                    RegionId = 1,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                },
                new District
                {
                    Id = 20,
                    ShortName = "D20",
                    FullName = "District 20",
                    RegionId = 2,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.Languages.AddRange(
                new Language
                {
                    Id = LanguageIdConst.UZ,
                    Code = "uz",
                    Name = "Uzbek",
                    NativeName = "O'zbek",
                    IsDefault = true,
                    SortOrder = 1,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                },
                new Language
                {
                    Id = LanguageIdConst.RU,
                    Code = "ru",
                    Name = "Russian",
                    NativeName = "Русский",
                    IsDefault = false,
                    SortOrder = 2,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.Organizations.Add(new Organization
            {
                Id = 710,
                ShortName = "ORG-710",
                FullName = "Organization 710",
                Inn = "710710710",
                RegionId = 1,
                DistrictId = 10,
                Address = "Old address",
                Director = "Old director",
                IsParent = false,
                DefaultLanguageId = LanguageIdConst.UZ,
                TenantId = 1,
                SetupStatus = "completed",
                SetupCompletedAt = new DateTime(2026, 1, 1),
                Email = "old@example.com",
                Website = "https://old.example.com",
                Oked = "11111",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        });

        using var client = CreateScopedClient(factory, userId: 101, organizationId: 1);
        var response = await client.PutAsJsonAsync("/api/organizations/710", new
        {
            shortName = "ORG-710-NEW",
            fullName = "Organization 710 Updated",
            inn = "710710710",
            phoneNumber = "998909999999",
            regionId = 2,
            districtId = 20,
            address = "New address",
            director = "New director",
            isParent = true,
            defaultLanguageId = LanguageIdConst.RU,
            tenantId = 2,
            setupStatus = "",
            setupCompletedAt = new DateTime(2026, 2, 1),
            email = "new@example.com",
            website = "https://new.example.com",
            oked = "22222",
            stateId = StateIdConst.PASSIVE
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);
        var organization = await db.Organizations.SingleAsync(x => x.Id == 710);

        Assert.Equal("ORG-710-NEW", organization.ShortName);
        Assert.Equal("Organization 710 Updated", organization.FullName);
        Assert.Equal("New address", organization.Address);
        Assert.Equal("New director", organization.Director);
        Assert.True(organization.IsParent);
        Assert.Equal(LanguageIdConst.RU, organization.DefaultLanguageId);
        Assert.Equal(2, organization.TenantId);
        Assert.Equal("completed", organization.SetupStatus);
        Assert.Equal(new DateTime(2026, 2, 1), organization.SetupCompletedAt);
        Assert.Equal(StateIdConst.PASSIVE, organization.StateId);
    }

    [Fact]
    public async Task OrganizationUpdate_ShouldRejectInnConflict()
    {
        await using var factory = CreateFactory();

        await SeedAsync(factory, async db =>
        {
            db.Organizations.AddRange(
                new Organization
                {
                    Id = 711,
                    ShortName = "ORG-711",
                    FullName = "Organization 711",
                    Inn = "711711711",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                },
                new Organization
                {
                    Id = 712,
                    ShortName = "ORG-712",
                    FullName = "Organization 712",
                    Inn = "712712712",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
        });

        using var client = CreateScopedClient(factory, userId: 101, organizationId: 1);
        var response = await client.PutAsJsonAsync("/api/organizations/711", new
        {
            shortName = "ORG-711",
            fullName = "Organization 711",
            inn = "712712712",
            phoneNumber = "998900000711",
            regionId = 1,
            districtId = (int?)null,
            address = "Address",
            director = "Director",
            isParent = true,
            defaultLanguageId = LanguageIdConst.UZ,
            tenantId = (int?)null,
            setupStatus = "pending",
            setupCompletedAt = (DateTime?)null,
            email = "org711@example.com",
            website = "https://org711.example.com",
            oked = "71111",
            stateId = StateIdConst.ACTIVE
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal("Organization.InnConflict", problem.Title);
    }

    [Fact]
    public async Task PlatformOrganizationUpdate_ShouldMapFields_AndPreserveSetupStatusWhenBlank()
    {
        await using var factory = CreateFactory();

        await SeedAsync(factory, async db =>
        {
            db.States.Add(new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "ACTIVE",
                FullName = "Active",
                CreatedDate = DateTime.UtcNow
            });

            db.Regions.AddRange(
                new Region
                {
                    Id = 1,
                    ShortName = "R1",
                    FullName = "Region 1",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                },
                new Region
                {
                    Id = 3,
                    ShortName = "R3",
                    FullName = "Region 3",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.Districts.AddRange(
                new District
                {
                    Id = 10,
                    ShortName = "D10",
                    FullName = "District 10",
                    RegionId = 1,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                },
                new District
                {
                    Id = 30,
                    ShortName = "D30",
                    FullName = "District 30",
                    RegionId = 3,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.Languages.AddRange(
                new Language
                {
                    Id = LanguageIdConst.UZ,
                    Code = "uz",
                    Name = "Uzbek",
                    NativeName = "O'zbek",
                    IsDefault = true,
                    SortOrder = 1,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                },
                new Language
                {
                    Id = LanguageIdConst.RU,
                    Code = "ru",
                    Name = "Russian",
                    NativeName = "Русский",
                    IsDefault = false,
                    SortOrder = 2,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.PlatformTenants.AddRange(
                new PlatformTenant
                {
                    Id = 801,
                    Name = "Tenant 801",
                    Slug = "tenant-801",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                },
                new PlatformTenant
                {
                    Id = 802,
                    Name = "Tenant 802",
                    Slug = "tenant-802",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.Organizations.Add(new Organization
            {
                Id = 720,
                ShortName = "ORG-720",
                FullName = "Organization 720",
                Inn = "720720720",
                RegionId = 1,
                DistrictId = 10,
                Address = "Old address",
                Director = "Old director",
                IsParent = false,
                DefaultLanguageId = LanguageIdConst.UZ,
                TenantId = 801,
                SetupStatus = "pending",
                SetupCompletedAt = null,
                Email = "old-platform@example.com",
                Website = "https://old-platform.example.com",
                Oked = "12345",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        });

        using var client = CreateGlobalClient(factory);
        var response = await client.PutAsJsonAsync("/api/platform/organizations/720", new
        {
            shortName = "ORG-720-NEW",
            fullName = "Organization 720 Updated",
            inn = "720720720",
            phoneNumber = "998901234720",
            regionId = 3,
            districtId = 30,
            address = "Platform address",
            director = "Platform director",
            isParent = true,
            defaultLanguageId = LanguageIdConst.RU,
            tenantId = 802,
            setupStatus = "",
            setupCompletedAt = new DateTime(2026, 3, 1),
            email = "new-platform@example.com",
            website = "https://new-platform.example.com",
            oked = "54321",
            stateId = StateIdConst.PASSIVE
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);
        var organization = await db.Organizations.SingleAsync(x => x.Id == 720);

        Assert.Equal("ORG-720-NEW", organization.ShortName);
        Assert.Equal("Organization 720 Updated", organization.FullName);
        Assert.Equal("Platform address", organization.Address);
        Assert.Equal("Platform director", organization.Director);
        Assert.True(organization.IsParent);
        Assert.Equal(802, organization.TenantId);
        Assert.Equal("pending", organization.SetupStatus);
        Assert.Equal(new DateTime(2026, 3, 1), organization.SetupCompletedAt);
        Assert.Equal(StateIdConst.PASSIVE, organization.StateId);
    }

    [Fact]
    public async Task OrganizationSetupFlow_ShouldAdvanceCurrentStateMachine_AndComplete()
    {
        await using var factory = CreateFactory();

        await SeedAsync(factory, async db =>
        {
            db.Roles.Add(new Role
            {
                Id = 31,
                ShortName = "SETUP_ROLE",
                FullName = "Setup Role",
                HasGlobalAccess = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.Users.Add(new User
            {
                Id = 501,
                UserName = "setup-user",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "998900005001",
                FirstName = "Setup",
                LastName = "User",
                RoleId = 31,
                OrganizationId = 730,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.Organizations.Add(new Organization
            {
                Id = 730,
                ShortName = "ORG-730",
                FullName = "Organization 730",
                Inn = "730730730",
                RegionId = 1,
                IsParent = true,
                StateId = StateIdConst.ACTIVE,
                SetupStatus = "pending",
                CreatedDate = DateTime.UtcNow
            });

            db.UserOrganizations.Add(new UserOrganization
            {
                UserId = 501,
                OrganizationId = 730,
                RoleId = 31,
                IsDefault = true,
                IsOwner = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            });

            db.TaxTypes.Add(new TaxType
            {
                Id = 2,
                Code = "SIMPLIFIED",
                Name = "Simplified",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.AccountingPolicies.Add(new AccountingPolicy
            {
                Id = 2,
                Code = "LOCAL",
                Name = "Local policy",
                StateId = StateIdConst.ACTIVE
            });

            db.Currencies.Add(new Currency
            {
                Id = 2,
                Code = "USD",
                Name = "US Dollar",
                StateId = StateIdConst.ACTIVE
            });

            db.Branches.Add(new Branch
            {
                Id = 731,
                OrganizationId = 730,
                Code = "BR-731",
                Name = "Main branch",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.Warehouses.Add(new Warehouse
            {
                Id = 732,
                OrganizationId = 730,
                BranchId = 731,
                Name = "Main warehouse",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.CashBoxes.Add(new CashBox
            {
                Id = 733,
                OrganizationId = 730,
                BranchId = 731,
                Code = "CB-733",
                Name = "Main cashbox",
                CurrencyId = 2,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.BankAccounts.Add(new BankAccount
            {
                Id = 734,
                OrganizationId = 730,
                BankId = 1,
                AccountNumber = "2020202020",
                CurrencyId = 2,
                IsMain = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.ChartAccounts.AddRange(
                CreateChartAccount(735, "REC"),
                CreateChartAccount(736, "PAY"),
                CreateChartAccount(737, "INV"),
                CreateChartAccount(738, "CASH"),
                CreateChartAccount(739, "BANK"),
                CreateChartAccount(740, "REV"),
                CreateChartAccount(741, "EXP"),
                CreateChartAccount(742, "COGS"));

            await db.SaveChangesAsync();
        });

        using var client = CreateScopedClient(factory, userId: 501, organizationId: 730, roleId: 31);

        var initial = await client.GetFromJsonAsync<OrganizationSetupDto>("/api/setup");
        Assert.NotNull(initial);
        Assert.Equal("company-profile", initial.CurrentStep);

        var companyResponse = await client.PutAsJsonAsync("/api/setup/company-profile", new
        {
            shortName = "ORG-730-UPD",
            fullName = "Organization 730 Updated",
            inn = "730730730",
            phoneNumber = "998907307307",
            regionId = 2,
            districtId = 20,
            address = "Setup address",
            director = "Setup director",
            defaultLanguageId = LanguageIdConst.RU,
            email = "setup@example.com",
            website = "https://setup.example.com",
            oked = "73000"
        });
        Assert.Equal(HttpStatusCode.NoContent, companyResponse.StatusCode);

        var afterCompany = await client.GetFromJsonAsync<OrganizationSetupDto>("/api/setup");
        Assert.NotNull(afterCompany);
        Assert.True(afterCompany.OrganizationCompleted);
        Assert.Equal("tax-settings", afterCompany.CurrentStep);

        var taxResponse = await client.PutAsJsonAsync("/api/setup/tax-settings", new
        {
            taxTypeId = 2,
            isVatPayer = false,
            vatRegistrationNumber = "SIMP-730",
            effectiveFrom = new DateOnly(2026, 1, 1),
            effectiveTo = (DateOnly?)null,
            stateId = StateIdConst.ACTIVE
        });
        Assert.Equal(HttpStatusCode.NoContent, taxResponse.StatusCode);

        var accountingResponse = await client.PutAsJsonAsync("/api/setup/accounting-policy", new
        {
            inventoryValuationMethod = "AVERAGE",
            accountingPolicyId = 2,
            baseCurrencyId = 2,
            accountingStartDate = new DateOnly(2026, 1, 1),
            fiscalYearStartMonth = 4
        });
        Assert.Equal(HttpStatusCode.NoContent, accountingResponse.StatusCode);

        var defaultsResponse = await client.PutAsJsonAsync("/api/setup/defaults", new
        {
            branchId = 731,
            warehouseId = 732,
            cashBoxId = 733,
            bankAccountId = 734,
            receivableAccountId = 735,
            payableAccountId = 736,
            inventoryAccountId = 737,
            cashAccountId = 738,
            bankAccountingAccountId = 739,
            revenueAccountId = 740,
            expenseAccountId = 741,
            cogsAccountId = 742
        });
        Assert.Equal(HttpStatusCode.NoContent, defaultsResponse.StatusCode);

        var usersResponse = await client.PutAsJsonAsync("/api/setup/users", new
        {
            usersCompleted = true
        });
        Assert.Equal(HttpStatusCode.NoContent, usersResponse.StatusCode);

        var completeResponse = await client.PostAsync("/api/setup/complete", content: null);
        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);

        var snapshot = await client.GetFromJsonAsync<OrganizationSetupDto>("/api/setup");
        Assert.NotNull(snapshot);
        Assert.Equal("complete", snapshot.CurrentStep);
        Assert.Equal("completed", snapshot.SetupStatus);
        Assert.True(snapshot.OrganizationCompleted);
        Assert.True(snapshot.TaxCompleted);
        Assert.True(snapshot.AccountingCompleted);
        Assert.True(snapshot.DefaultsCompleted);
        Assert.True(snapshot.UsersCompleted);
        Assert.True(snapshot.IsCompleted);
        Assert.Equal("ORG-730-UPD", snapshot.CompanyProfile.ShortName);
        Assert.Equal((short)2, snapshot.TaxSettings!.TaxTypeId);
        Assert.Equal("average", snapshot.AccountingPolicy!.InventoryValuationMethod);
        Assert.Equal(731, snapshot.Defaults!.BranchId);
        Assert.Single(snapshot.Users);
        Assert.Equal(501, snapshot.Users.Single().UserId);
    }

    [Fact]
    public async Task AuditLogs_RecordView_ShouldReturnChangeResultsSnapshot()
    {
        await using var factory = CreateFactory();

        await SeedAsync(factory, async db =>
        {
            db.AuditLogs.Add(new AuditLog
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
            });

            await db.SaveChangesAsync();
        });

        using var client = CreateScopedClient(factory, userId: 101, organizationId: 1);
        var response = await client.GetAsync("/api/audit-logs?tableName=snapshot_doc&recordId=42");

        response.EnsureSuccessStatusCode();
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();

        Assert.NotNull(logs);
        Assert.Single(logs);
        Assert.Contains(logs.Single().ChangeResults, x => x.Path == "status" && Equals(x.OldValue?.ToString(), "draft") && Equals(x.NewValue?.ToString(), "posted"));
        Assert.Contains(logs.Single().ChangeResults, x => x.Path == "amount" && Equals(x.OldValue?.ToString(), "10") && Equals(x.NewValue?.ToString(), "12"));
    }

    [Fact]
    public async Task PlatformAuditLogs_GlobalUser_ShouldReturnCurrentGlobalProjection()
    {
        await using var factory = CreateFactory();

        await SeedAsync(factory, async db =>
        {
            db.Users.AddRange(
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
                    StateId = StateIdConst.ACTIVE,
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
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow
                });

            db.Organizations.AddRange(
                new Organization
                {
                    Id = 901,
                    ShortName = "AUD-ORG-1",
                    FullName = "Audit Organization 1",
                    Inn = "901901901",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
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
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                });

            db.AuditLogs.AddRange(
                new AuditLog
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
                new AuditLog
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
                });

            await db.SaveChangesAsync();
        });

        using var client = CreateGlobalClient(factory);
        var response = await client.GetAsync("/api/platform/audit-logs?tableName=workspace_bootstrap&recordId=100");

        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<PlatformAuditLogDto>>();

        Assert.NotNull(page);
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(new long[] { 1102, 1101 }, page.Items.Select(x => x.Id).ToArray());
        Assert.Equal("AUD-ORG-2", page.Items.First().OrganizationName);
        Assert.Equal("global-auditor-2", page.Items.First().ChangedUserName);
        Assert.Equal("AUD-ORG-1", page.Items.Last().OrganizationName);
        Assert.Equal("global-auditor-1", page.Items.Last().ChangedUserName);
    }

    [Fact]
    public async Task PlatformAuditLogs_NonGlobalUser_ShouldBeForbidden()
    {
        await using var factory = CreateFactory();
        using var client = CreateScopedClient(factory, userId: 101, organizationId: 1);

        var response = await client.GetAsync("/api/platform/audit-logs");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static TestWebApplicationFactory CreateFactory()
    {
        var emailSender = new RecordingEmailSender();
        var databaseName = $"superadmin-safety-{Guid.NewGuid():N}";

        return new TestWebApplicationFactory(overrideServices: services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            services.RemoveAll(typeof(IEmailSender));
            services.AddSingleton<IEmailSender>(emailSender);
        });
    }

    private static HttpClient CreateScopedClient(TestWebApplicationFactory factory, int userId, int organizationId, int roleId = 1)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-RoleId", roleId.ToString());
        client.DefaultRequestHeaders.Add("X-OrganizationId", organizationId.ToString());
        return client;
    }

    private static HttpClient CreateGlobalClient(TestWebApplicationFactory factory, int userId = 900, int roleId = 1)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-RoleId", roleId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-GlobalAccess", "true");
        return client;
    }

    private static async Task SeedAsync(TestWebApplicationFactory factory, Func<AppDbContext, Task> seed)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await seed(db);
    }

    private static void SetGlobalUserContext(AppDbContext db) =>
        db.SetUserContext(new FakeUserContext
        {
            Id = 1,
            RoleId = 1,
            HasGlobalAccess = true
        });

    private static ChartAccount CreateChartAccount(int id, string code) =>
        new()
        {
            Id = id,
            Code = code,
            Number = code,
            Name = code,
            IsGroup = false,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.UtcNow
        };
}
