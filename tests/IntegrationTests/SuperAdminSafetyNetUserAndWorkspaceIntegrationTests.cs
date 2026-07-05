using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Integration;
using Application.Features.Platform;
using Application.Features.Users;
using Application.Features.Users.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;

namespace IntegrationTests;

public sealed class SuperAdminSafetyNetUserAndWorkspaceIntegrationTests
{
    [Fact]
    public async Task UserCreate_ShouldSendWelcomeEmail_AndPersistCurrentMembershipBehavior()
    {
        var emailSender = new RecordingEmailSender();
        await using var factory = CreateFactory(emailSender);

        await SeedAsync(factory, async db =>
        {
            db.UserOrganizations.AddRange(
                new UserOrganization
                {
                    UserId = 999,
                    OrganizationId = 1,
                    IsDefault = true,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow,
                    JoinedAt = DateTime.UtcNow
                },
                new UserOrganization
                {
                    UserId = 999,
                    OrganizationId = 2,
                    IsDefault = false,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow,
                    JoinedAt = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
        });

        using var client = CreateOrgAwareClient(factory, userId: 999);
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            userName = "snapshot.sys.user",
            password = "Secret123!",
            phoneNumber = "998901234567",
            email = "snapshot-sys@example.com",
            firstName = "Snapshot",
            lastName = "User",
            roleId = 7,
            emailVerified = false,
            isPlatformAdmin = false,
            timezone = "Asia/Tashkent",
            organizations = new[] { 1, 2, 1 }
        });

        response.EnsureSuccessStatusCode();
        var userId = await response.Content.ReadFromJsonAsync<int>();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);

        var user = await db.Users.SingleAsync(x => x.Id == userId);
        var memberships = await db.UserOrganizations
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.OrganizationId)
            .ToListAsync();

        Assert.Equal(StateIdConst.ACTIVE, user.StateId);
        Assert.Equal(new[] { 1, 2 }, memberships.Select(x => x.OrganizationId).ToArray());
        Assert.True(memberships.Single(x => x.OrganizationId == 1).IsDefault);
        Assert.False(memberships.Single(x => x.OrganizationId == 2).IsDefault);
        Assert.All(memberships, x => Assert.Equal(StateIdConst.ACTIVE, x.StateId));

        Assert.Single(emailSender.Messages);
        Assert.Equal(["snapshot-sys@example.com"], emailSender.Messages.Single().To);
    }

    [Fact]
    public async Task UserUpdate_ShouldReplaceMemberships_AndApplyRequestedState()
    {
        var originalMembershipDate = new DateTime(2020, 1, 1);
        var users = new List<User>
        {
            new()
            {
                Id = 610,
                UserName = "legacy-user",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "998900000610",
                FirstName = "Legacy",
                LastName = "User",
                RoleId = 1,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            }
        };
        var memberships = new List<UserOrganization>
        {
            new()
            {
                UserId = 610,
                OrganizationId = 1,
                IsDefault = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = originalMembershipDate,
                JoinedAt = originalMembershipDate
            },
            new()
            {
                UserId = 610,
                OrganizationId = 2,
                IsDefault = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = originalMembershipDate,
                JoinedAt = originalMembershipDate
            }
        };

        var emailSender = new RecordingEmailSender();
        var userContext = new SnapshotUserContext(999, [1, 2]);
        var userCore = new UserManagementCore(
            userContext,
            new SnapshotPasswordHasher(),
            new SnapshotQueryRepository<User>(users),
            new SnapshotCommandRepository<User>(users),
            new SnapshotQueryRepository<UserOrganization>(memberships),
            new SnapshotCommandRepository<UserOrganization>(memberships),
            new SnapshotQueryRepository<Role>([]),
            new SnapshotQueryRepository<Organization>([]),
            emailSender,
            NullLogger<UserManagementCore>.Instance);
        var service = new UserService(
            userContext,
            new SnapshotQueryBuilder(),
            new SnapshotQueryRepository<User>(users),
            new SnapshotCommandRepository<User>(users),
            new SnapshotQueryRepository<UserOrganization>(memberships),
            userCore,
            NullLogger<UserService>.Instance,
            new SnapshotUnitOfWork());

        var result = await service.UpdateAsync(610, new UserUpdateDto
        {
            UserName = "updated-user",
            PhoneNumber = "998900000999",
            Email = "updated-user@example.com",
            FirstName = "Updated",
            LastName = "Snapshot",
            RoleId = 1,
            EmailVerified = true,
            IsPlatformAdmin = true,
            Timezone = "UTC",
            StateId = StateIdConst.PASSIVE,
            Organizations = [1, 2, 2]
        });

        Assert.True(result.IsSuccess);

        var user = users.Single(x => x.Id == 610);
        var userMemberships = memberships
            .Where(x => x.UserId == 610)
            .OrderBy(x => x.OrganizationId)
            .ToList();

        Assert.Equal("updated-user", user.UserName);
        Assert.Equal(StateIdConst.PASSIVE, user.StateId);
        Assert.True(user.EmailVerified);
        Assert.True(user.IsPlatformAdmin);
        Assert.Equal(new[] { 1, 2 }, userMemberships.Select(x => x.OrganizationId).ToArray());
        Assert.True(userMemberships.Single(x => x.OrganizationId == 1).IsDefault);
        Assert.False(userMemberships.Single(x => x.OrganizationId == 2).IsDefault);
        Assert.All(userMemberships, x => Assert.Equal(StateIdConst.ACTIVE, x.StateId));
        Assert.All(userMemberships, x => Assert.True(x.CreatedDate > originalMembershipDate));
        Assert.Empty(emailSender.Messages);
    }

    [Fact]
    public async Task PlatformUserCreate_ShouldNotSendWelcomeEmail_AndNormalizeMemberships()
    {
        var emailSender = new RecordingEmailSender();
        await using var factory = CreateFactory(emailSender);

        await SeedAsync(factory, async db =>
        {
            db.Roles.Add(new Role
            {
                Id = 11,
                ShortName = "PLATFORM_USER",
                FullName = "Platform User",
                HasGlobalAccess = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.Organizations.AddRange(
                new Organization
                {
                    Id = 210,
                    ShortName = "ORG-210",
                    FullName = "Organization 210",
                    Inn = "210210210",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                },
                new Organization
                {
                    Id = 220,
                    ShortName = "ORG-220",
                    FullName = "Organization 220",
                    Inn = "220220220",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
        });

        using var client = CreateGlobalClient(factory);
        var response = await client.PostAsJsonAsync("/api/platform/users", new
        {
            userName = " platform.snapshot ",
            password = "Secret123!",
            phoneNumber = " 998991112233 ",
            email = "platform-snapshot@example.com",
            firstName = " Platform ",
            lastName = " Snapshot ",
            roleId = 11,
            languageId = LanguageIdConst.UZ,
            emailVerified = true,
            isPlatformAdmin = true,
            timezone = "Asia/Tashkent",
            organizations = new object[]
            {
                new { organizationId = 210, roleId = 11, isDefault = false, isOwner = false, invitedByUserId = (int?)null },
                new { organizationId = 220, roleId = 11, isDefault = true, isOwner = true, invitedByUserId = (int?)null },
                new { organizationId = 220, roleId = 11, isDefault = false, isOwner = false, invitedByUserId = (int?)null }
            }
        });

        response.EnsureSuccessStatusCode();
        var userId = await response.Content.ReadFromJsonAsync<int>();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);

        var user = await db.Users.SingleAsync(x => x.Id == userId);
        var memberships = await db.UserOrganizations
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.OrganizationId)
            .ToListAsync();

        Assert.Equal("platform.snapshot", user.UserName);
        Assert.Equal("998991112233", user.PhoneNumber);
        Assert.Equal("Platform", user.FirstName);
        Assert.Equal("Snapshot", user.LastName);
        Assert.Equal(StateIdConst.ACTIVE, user.StateId);
        Assert.Equal(220, user.OrganizationId);
        Assert.NotNull(user.EmailVerifiedAt);
        Assert.Equal(new[] { 210, 220 }, memberships.Select(x => x.OrganizationId).ToArray());
        Assert.False(memberships.Single(x => x.OrganizationId == 210).IsDefault);
        Assert.True(memberships.Single(x => x.OrganizationId == 220).IsDefault);
        Assert.True(memberships.Single(x => x.OrganizationId == 220).IsOwner);
        Assert.Empty(emailSender.Messages);
    }

    [Fact]
    public async Task PlatformUserCreate_ShouldReturnConflict_ForDuplicateUserName()
    {
        var emailSender = new RecordingEmailSender();
        await using var factory = CreateFactory(emailSender);

        await SeedAsync(factory, async db =>
        {
            db.Roles.Add(new Role
            {
                Id = 12,
                ShortName = "ROLE-12",
                FullName = "Role 12",
                HasGlobalAccess = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.Organizations.Add(new Organization
            {
                Id = 230,
                ShortName = "ORG-230",
                FullName = "Organization 230",
                Inn = "230230230",
                RegionId = 1,
                IsParent = true,
                StateId = StateIdConst.ACTIVE,
                SetupStatus = "pending",
                CreatedDate = DateTime.UtcNow
            });

            db.Users.Add(new User
            {
                Id = 620,
                UserName = "duplicate-user",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "998900000620",
                FirstName = "Duplicate",
                LastName = "Owner",
                RoleId = 12,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        });

        using var client = CreateGlobalClient(factory);
        var response = await client.PostAsJsonAsync("/api/platform/users", new
        {
            userName = "duplicate-user",
            password = "Secret123!",
            phoneNumber = "998900001111",
            email = "dup@example.com",
            firstName = "Dup",
            lastName = "Again",
            roleId = 12,
            emailVerified = false,
            isPlatformAdmin = false,
            timezone = "UTC",
            organizations = new object[]
            {
                new { organizationId = 230, roleId = 12, isDefault = true, isOwner = false, invitedByUserId = (int?)null }
            }
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal("PlatformUser.UserNameConflict", problem.Title);
        Assert.Empty(emailSender.Messages);
    }

    [Fact]
    public async Task PlatformUserUpdate_ShouldReplaceMemberships_AndFreezeCurrentStateHandling()
    {
        var emailSender = new RecordingEmailSender();
        await using var factory = CreateFactory(emailSender);

        await SeedAsync(factory, async db =>
        {
            db.Roles.Add(new Role
            {
                Id = 13,
                ShortName = "ROLE-13",
                FullName = "Role 13",
                HasGlobalAccess = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.Organizations.AddRange(
                new Organization
                {
                    Id = 310,
                    ShortName = "ORG-310",
                    FullName = "Organization 310",
                    Inn = "310310310",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                },
                new Organization
                {
                    Id = 320,
                    ShortName = "ORG-320",
                    FullName = "Organization 320",
                    Inn = "320320320",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                },
                new Organization
                {
                    Id = 330,
                    ShortName = "ORG-330",
                    FullName = "Organization 330",
                    Inn = "330330330",
                    RegionId = 1,
                    IsParent = true,
                    StateId = StateIdConst.ACTIVE,
                    SetupStatus = "pending",
                    CreatedDate = DateTime.UtcNow
                });

            db.Users.Add(new User
            {
                Id = 630,
                UserName = "platform-legacy",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "998900000630",
                FirstName = "Legacy",
                LastName = "Platform",
                RoleId = 13,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.UserOrganizations.AddRange(
                new UserOrganization
                {
                    UserId = 630,
                    OrganizationId = 310,
                    RoleId = 13,
                    IsDefault = true,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow,
                    JoinedAt = DateTime.UtcNow
                },
                new UserOrganization
                {
                    UserId = 630,
                    OrganizationId = 320,
                    RoleId = 13,
                    IsDefault = false,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.UtcNow,
                    JoinedAt = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
        });

        using var client = CreateGlobalClient(factory);
        var response = await client.PutAsJsonAsync("/api/platform/users/630", new
        {
            userName = " platform-updated ",
            phoneNumber = " 998900009999 ",
            email = "platform-updated@example.com",
            firstName = " Updated ",
            lastName = " Snapshot ",
            roleId = 13,
            languageId = LanguageIdConst.RU,
            emailVerified = true,
            isPlatformAdmin = false,
            timezone = "UTC",
            stateId = StateIdConst.PASSIVE,
            organizations = new object[]
            {
                new { organizationId = 330, roleId = 13, isDefault = false, isOwner = true, invitedByUserId = (int?)null }
            }
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);

        var user = await db.Users.SingleAsync(x => x.Id == 630);
        var memberships = await db.UserOrganizations
            .Where(x => x.UserId == 630)
            .OrderBy(x => x.OrganizationId)
            .ToListAsync();

        Assert.Equal("platform-updated", user.UserName);
        Assert.Equal("998900009999", user.PhoneNumber);
        Assert.Equal("Updated", user.FirstName);
        Assert.Equal("Snapshot", user.LastName);
        Assert.Equal(StateIdConst.PASSIVE, user.StateId);
        Assert.Equal(330, user.OrganizationId);
        Assert.NotNull(user.EmailVerifiedAt);

        Assert.Equal(3, memberships.Count);
        Assert.Equal(StateIdConst.PASSIVE, memberships.Single(x => x.OrganizationId == 310).StateId);
        Assert.Equal(StateIdConst.PASSIVE, memberships.Single(x => x.OrganizationId == 320).StateId);
        Assert.NotNull(memberships.Single(x => x.OrganizationId == 310).BlockedAt);
        Assert.NotNull(memberships.Single(x => x.OrganizationId == 320).BlockedAt);
        Assert.Equal(StateIdConst.ACTIVE, memberships.Single(x => x.OrganizationId == 330).StateId);
        Assert.True(memberships.Single(x => x.OrganizationId == 330).IsDefault);
        Assert.True(memberships.Single(x => x.OrganizationId == 330).IsOwner);
        Assert.Empty(emailSender.Messages);
    }

    [Fact]
    public async Task PlatformAccountantWorkspaceCreate_ShouldBootstrapCurrentCompletedFlow()
    {
        var emailSender = new RecordingEmailSender();
        await using var factory = CreateFactory(emailSender);

        await SeedAsync(factory, async db =>
        {
            db.Roles.Add(new Role
            {
                Id = 21,
                ShortName = "WORKSPACE_ADMIN",
                FullName = "Workspace Admin",
                HasGlobalAccess = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.TaxTypes.Add(new TaxType
            {
                Id = 1,
                Code = "VAT",
                Name = "VAT",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            db.AccountingPolicies.Add(new AccountingPolicy
            {
                Id = 1,
                Code = "STD",
                Name = "Standard",
                StateId = StateIdConst.ACTIVE
            });

            db.Currencies.Add(new Currency
            {
                Id = 1,
                Code = "UZS",
                Name = "Uzbek sum",
                StateId = StateIdConst.ACTIVE
            });

            await db.SaveChangesAsync();
        });

        using var client = CreateGlobalClient(factory);
        var response = await client.PostAsJsonAsync("/api/platform/accountant-workspaces", new
        {
            tenantName = "Snapshot Tenant",
            tenantSlug = "snapshot-tenant",
            userName = "workspace.owner",
            password = "Secret123!",
            phoneNumber = "998901110000",
            email = "workspace-owner@example.com",
            firstName = "Workspace",
            lastName = "Owner",
            roleId = 21,
            languageId = LanguageIdConst.UZ,
            timezone = "Asia/Tashkent",
            organizationShortName = "Snapshot Org",
            organizationFullName = "Snapshot Organization LLC",
            inn = "555666777",
            organizationPhoneNumber = "998901112233",
            regionId = 1,
            districtId = (int?)null,
            address = "Tashkent",
            director = "Director",
            defaultLanguageId = LanguageIdConst.UZ,
            organizationEmail = "org@example.com",
            website = "https://example.com",
            oked = "62010",
            taxTypeId = 1,
            isVatPayer = true,
            vatRegistrationNumber = "VAT-555",
            taxEffectiveFrom = new DateOnly(2026, 1, 1),
            accountingPolicyId = 1,
            baseCurrencyId = 1,
            accountingStartDate = new DateOnly(2026, 1, 1),
            inventoryValuationMethod = "FIFO",
            fiscalYearStartMonth = 3,
            defaults = new
            {
                branchId = 101,
                warehouseId = 102,
                cashBoxId = 103,
                bankAccountId = 104,
                receivableAccountId = 105,
                payableAccountId = 106,
                inventoryAccountId = 107,
                cashAccountId = 108,
                bankAccountingAccountId = 109,
                revenueAccountId = 110,
                expenseAccountId = 111,
                cogsAccountId = 112
            },
            completeSetup = true
        });

        response.EnsureSuccessStatusCode();
        var workspace = await response.Content.ReadFromJsonAsync<AccountantWorkspaceDto>();

        Assert.NotNull(workspace);
        Assert.Equal("snapshot-tenant", workspace.Tenant.Slug);
        Assert.Equal("completed", workspace.Organization.SetupStatus);
        Assert.Equal("complete", workspace.Setup.CurrentStep);
        Assert.True(workspace.Setup.IsCompleted);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);

        var tenant = await db.PlatformTenants.SingleAsync(x => x.Slug == "snapshot-tenant");
        var organization = await db.Organizations.SingleAsync(x => x.TenantId == tenant.Id);
        var user = await db.Users.SingleAsync(x => x.UserName == "workspace.owner");
        var membership = await db.UserOrganizations.SingleAsync(x => x.UserId == user.Id && x.OrganizationId == organization.Id);
        var tax = await db.OrganizationTaxSettings.SingleAsync(x => x.OrganizationId == organization.Id);
        var config = await db.OrganizationConfigs.SingleAsync(x => x.OrganizationId == organization.Id);
        var defaults = await db.OrganizationDefaults.SingleAsync(x => x.OrganizationId == organization.Id);
        var setup = await db.OrganizationSetupStates.SingleAsync(x => x.OrganizationId == organization.Id);

        Assert.Equal(user.Id, tenant.OwnerUserId);
        Assert.Equal(organization.Id, user.OrganizationId);
        Assert.True(user.EmailVerified);
        Assert.NotNull(user.EmailVerifiedAt);
        Assert.True(membership.IsDefault);
        Assert.True(membership.IsOwner);
        Assert.Equal(StateIdConst.ACTIVE, membership.StateId);
        Assert.Equal((short)1, tax.TaxTypeId);
        Assert.Equal("fifo", config.InventoryValuationMethod);
        Assert.Equal((short)3, config.FiscalYearStartMonth);
        Assert.Equal(101, defaults.BranchId);
        Assert.Equal(112, defaults.CogsAccountId);
        Assert.Equal("complete", setup.CurrentStep);
        Assert.True(setup.OrganizationCompleted);
        Assert.True(setup.TaxCompleted);
        Assert.True(setup.AccountingCompleted);
        Assert.True(setup.DefaultsCompleted);
        Assert.True(setup.UsersCompleted);
        Assert.True(setup.IsCompleted);
        Assert.NotNull(setup.CompletedAt);
        Assert.Empty(emailSender.Messages);
    }

    private static TestWebApplicationFactory CreateFactory(RecordingEmailSender emailSender)
    {
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

    private static HttpClient CreateOrgAwareClient(TestWebApplicationFactory factory, int userId, int roleId = 1)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-RoleId", roleId.ToString());
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
}

file sealed class SnapshotUserContext(int userId, List<int> allowedOrganizationIds) : Application.Abstractions.Authentication.IUserContext
{
    public int? Id => userId;
    public int? RoleId => 1;
    public short? LanguageId => LanguageIdConst.EN;
    public int? OrganizationId => null;
    public List<int> AllowedOrganizationIds => allowedOrganizationIds;
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class SnapshotUnitOfWork : Application.Abstractions.IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class SnapshotPasswordHasher : Application.Abstractions.Authentication.IPasswordHasher
{
    public string GenerateSalt() => "salt";
    public string Hash(string password, string passwordSalt) => $"{password}:{passwordSalt}";
    public bool Verify(string password, string passwordSalt, string passwordHash) => passwordHash == Hash(password, passwordSalt);
}

file sealed class SnapshotQueryBuilder : IQueryBuilder
{
    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = new SnapshotQueryBuilderResolver() });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        throw new NotSupportedException();

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        throw new NotSupportedException();

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : IPaginationFilter =>
        throw new NotSupportedException();

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : IPaginationFilter =>
        throw new NotSupportedException();
}

file sealed class SnapshotQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
        throw new NotSupportedException();
}

file sealed class SnapshotCommandRepository<TEntity>(List<TEntity> items) : Application.Abstractions.ICommandRepository<TEntity>
    where TEntity : class
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

    public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        items.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities.ToList())
            items.Remove(entity);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        var compiled = predicate.Compile();
        items.RemoveAll(x => compiled(x));
        return Task.CompletedTask;
    }

    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;

    private static void AssignIdentityIfNeeded(TEntity entity)
    {
        var property = typeof(TEntity).GetProperty("Id");
        if (property is null)
            return;

        if (property.PropertyType == typeof(int))
        {
            var currentValue = (int?)property.GetValue(entity) ?? 0;
            if (currentValue == 0)
                property.SetValue(entity, (int)(DateTime.UtcNow.Ticks % int.MaxValue));
            return;
        }

        if (property.PropertyType == typeof(long))
        {
            var currentValue = (long?)property.GetValue(entity) ?? 0L;
            if (currentValue == 0)
                property.SetValue(entity, DateTime.UtcNow.Ticks);
        }
    }
}

file sealed class SnapshotQueryRepository<TEntity>(IEnumerable<TEntity> source) : Application.Abstractions.IQueryRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _items = source as List<TEntity> ?? source.ToList();

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
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
