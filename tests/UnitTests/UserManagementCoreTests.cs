using System.Linq.Expressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Application.Features.Users.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace UnitTests;

public sealed class UserManagementCoreTests
{
    [Fact]
    public async Task CreateUserAsync_OrganizationScope_ShouldKeepWelcomeEmailPolicy_AndDistinctMemberships()
    {
        var users = new List<User>();
        var memberships = new List<UserOrganization>();
        var emailSender = new RecordingEmailSender();
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = false, LanguageId = LanguageIdConst.EN },
            users,
            memberships,
            [],
            [],
            emailSender);

        var result = await core.CreateUserAsync(
            new UserManagementCreateRequest
            {
                UserName = " raw.sys.user ",
                Password = "Secret123!",
                PhoneNumber = "998901234567",
                Email = "sys@example.com",
                FirstName = "System",
                LastName = "User",
                RoleId = 7,
                EmailVerified = true,
                IsPlatformAdmin = true,
                Timezone = "Asia/Tashkent",
                Organizations =
                [
                    new UserManagementMembershipRequest { OrganizationId = 1 },
                    new UserManagementMembershipRequest { OrganizationId = 2 },
                    new UserManagementMembershipRequest { OrganizationId = 1 }
                ]
            },
            UserManagementOptions.ForOrganization(sendWelcomeEmail: true));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.WelcomeEmail);
        Assert.Equal("sys@example.com", result.Value.WelcomeEmail!.Email);

        var user = users.Single();
        Assert.Equal(" raw.sys.user ", user.UserName);
        Assert.Null(user.EmailVerifiedAt);
        Assert.Null(user.OrganizationId);
        Assert.Null(user.LanguageId);
        Assert.Equal(StateIdConst.ACTIVE, user.StateId);

        var createdMemberships = memberships.OrderBy(x => x.OrganizationId).ToList();
        Assert.Equal(new[] { 1, 2 }, createdMemberships.Select(x => x.OrganizationId).ToArray());
        Assert.True(createdMemberships.Single(x => x.OrganizationId == 1).IsDefault);
        Assert.False(createdMemberships.Single(x => x.OrganizationId == 2).IsDefault);
        Assert.All(createdMemberships, x => Assert.Equal(StateIdConst.ACTIVE, x.StateId));
        Assert.All(createdMemberships, x => Assert.NotEqual(default, x.JoinedAt));
        Assert.Empty(emailSender.Messages);
    }

    [Fact]
    public async Task CreateUserAsync_GlobalScope_ShouldTrimFields_AndSuppressWelcomeEmail()
    {
        var users = new List<User>();
        var memberships = new List<UserOrganization>();
        var roles = new List<Role>
        {
            new()
            {
                Id = 11,
                ShortName = "ROLE-11",
                FullName = "Role 11",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            }
        };
        var organizations = new List<Organization>
        {
            new()
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
            new()
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
            }
        };
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = true },
            users,
            memberships,
            roles,
            organizations,
            new RecordingEmailSender());

        var result = await core.CreateUserAsync(
            new UserManagementCreateRequest
            {
                UserName = " platform.user ",
                Password = "Secret123!",
                PhoneNumber = " 998991112233 ",
                Email = "platform@example.com",
                FirstName = " Platform ",
                LastName = " Snapshot ",
                RoleId = 11,
                LanguageId = LanguageIdConst.UZ,
                EmailVerified = true,
                IsPlatformAdmin = true,
                Timezone = "Asia/Tashkent",
                Organizations =
                [
                    new UserManagementMembershipRequest { OrganizationId = 210, RoleId = 11, IsDefault = false },
                    new UserManagementMembershipRequest { OrganizationId = 220, RoleId = 11, IsDefault = true, IsOwner = true },
                    new UserManagementMembershipRequest { OrganizationId = 220, RoleId = 11, IsDefault = false }
                ]
            },
            UserManagementOptions.ForGlobal());

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.WelcomeEmail);

        var user = users.Single();
        Assert.Equal("platform.user", user.UserName);
        Assert.Equal("998991112233", user.PhoneNumber);
        Assert.Equal("Platform", user.FirstName);
        Assert.Equal("Snapshot", user.LastName);
        Assert.Equal(LanguageIdConst.UZ, user.LanguageId);
        Assert.Equal(220, user.OrganizationId);
        Assert.True(user.EmailVerified);
        Assert.NotNull(user.EmailVerifiedAt);

        var createdMemberships = memberships.OrderBy(x => x.OrganizationId).ToList();
        Assert.Equal(new[] { 210, 220 }, createdMemberships.Select(x => x.OrganizationId).ToArray());
        Assert.False(createdMemberships.Single(x => x.OrganizationId == 210).IsDefault);
        Assert.True(createdMemberships.Single(x => x.OrganizationId == 220).IsDefault);
        Assert.True(createdMemberships.Single(x => x.OrganizationId == 220).IsOwner);
    }

    [Fact]
    public async Task UpdateUserAsync_GlobalScope_ShouldReplaceMemberships_AndKeepPassiveHistory()
    {
        var users = new List<User>
        {
            new()
            {
                Id = 630,
                UserName = "legacy-platform",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "998900000630",
                FirstName = "Legacy",
                LastName = "Platform",
                RoleId = 13,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            }
        };
        var memberships = new List<UserOrganization>
        {
            new()
            {
                UserId = 630,
                OrganizationId = 310,
                RoleId = 13,
                IsDefault = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            },
            new()
            {
                UserId = 630,
                OrganizationId = 320,
                RoleId = 13,
                IsDefault = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            }
        };
        var roles = new List<Role>
        {
            new()
            {
                Id = 13,
                ShortName = "ROLE-13",
                FullName = "Role 13",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            }
        };
        var organizations = new List<Organization>
        {
            new() { Id = 310, ShortName = "ORG-310", FullName = "Organization 310", Inn = "310310310", RegionId = 1, IsParent = true, StateId = StateIdConst.ACTIVE, SetupStatus = "pending", CreatedDate = DateTime.UtcNow },
            new() { Id = 320, ShortName = "ORG-320", FullName = "Organization 320", Inn = "320320320", RegionId = 1, IsParent = true, StateId = StateIdConst.ACTIVE, SetupStatus = "pending", CreatedDate = DateTime.UtcNow },
            new() { Id = 330, ShortName = "ORG-330", FullName = "Organization 330", Inn = "330330330", RegionId = 1, IsParent = true, StateId = StateIdConst.ACTIVE, SetupStatus = "pending", CreatedDate = DateTime.UtcNow }
        };
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = true },
            users,
            memberships,
            roles,
            organizations,
            new RecordingEmailSender());

        var result = await core.UpdateUserAsync(
            new UserManagementUpdateRequest
            {
                UserId = 630,
                UserName = " platform-updated ",
                PhoneNumber = " 998900009999 ",
                Email = "platform-updated@example.com",
                FirstName = " Updated ",
                LastName = " Snapshot ",
                RoleId = 13,
                LanguageId = LanguageIdConst.RU,
                EmailVerified = true,
                IsPlatformAdmin = false,
                Timezone = "UTC",
                StateId = StateIdConst.PASSIVE,
                Organizations =
                [
                    new UserManagementMembershipRequest
                    {
                        OrganizationId = 330,
                        RoleId = 13,
                        IsDefault = false,
                        IsOwner = true
                    }
                ]
            },
            UserManagementOptions.ForGlobal());

        Assert.True(result.IsSuccess);

        var user = users.Single();
        Assert.Equal("platform-updated", user.UserName);
        Assert.Equal("998900009999", user.PhoneNumber);
        Assert.Equal("Updated", user.FirstName);
        Assert.Equal("Snapshot", user.LastName);
        Assert.Equal(StateIdConst.PASSIVE, user.StateId);
        Assert.Equal(330, user.OrganizationId);
        Assert.NotNull(user.EmailVerifiedAt);

        var updatedMemberships = memberships.OrderBy(x => x.OrganizationId).ToList();
        Assert.Equal(3, updatedMemberships.Count);
        Assert.Equal(StateIdConst.PASSIVE, updatedMemberships.Single(x => x.OrganizationId == 310).StateId);
        Assert.Equal(StateIdConst.PASSIVE, updatedMemberships.Single(x => x.OrganizationId == 320).StateId);
        Assert.NotNull(updatedMemberships.Single(x => x.OrganizationId == 310).BlockedAt);
        Assert.NotNull(updatedMemberships.Single(x => x.OrganizationId == 320).BlockedAt);
        Assert.Equal(StateIdConst.ACTIVE, updatedMemberships.Single(x => x.OrganizationId == 330).StateId);
        Assert.True(updatedMemberships.Single(x => x.OrganizationId == 330).IsDefault);
        Assert.True(updatedMemberships.Single(x => x.OrganizationId == 330).IsOwner);
    }

    private static UserManagementCore CreateCore(
        FakeUserContext userContext,
        List<User> users,
        List<UserOrganization> memberships,
        List<Role> roles,
        List<Organization> organizations,
        RecordingEmailSender emailSender) =>
        new(
            userContext,
            new FakePasswordHasher(),
            new InMemoryQueryRepository<User>(users),
            new InMemoryCommandRepository<User>(users),
            new InMemoryQueryRepository<UserOrganization>(memberships),
            new InMemoryCommandRepository<UserOrganization>(memberships),
            new InMemoryQueryRepository<Role>(roles),
            new InMemoryQueryRepository<Organization>(organizations),
            emailSender,
            NullLogger<UserManagementCore>.Instance);
}

sealed class FakeUserContext : IUserContext
{
    public int? Id { get; init; } = 1;
    public int? RoleId { get; init; } = 1;
    public short? LanguageId { get; init; } = LanguageIdConst.EN;
    public int? OrganizationId { get; init; }
    public List<int> AllowedOrganizationIds { get; init; } = [];
    public int? BranchId { get; init; }
    public bool HasGlobalAccess { get; init; }
}

sealed class FakePasswordHasher : IPasswordHasher
{
    public string GenerateSalt() => "salt";
    public string Hash(string password, string passwordSalt) => $"{password}:{passwordSalt}";
    public bool Verify(string password, string passwordSalt, string passwordHash) => passwordHash == Hash(password, passwordSalt);
}

sealed class RecordingEmailSender : IEmailSender
{
    public List<EmailMessage> Messages { get; } = [];

    public Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        Messages.Add(message);
        return Task.FromResult(Result.Success());
    }
}

sealed class InMemoryCommandRepository<TEntity>(List<TEntity> items) : ICommandRepository<TEntity>
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

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        var compiled = predicate.Compile();
        items.RemoveAll(entity => compiled(entity));
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
        }
    }
}

sealed class InMemoryQueryRepository<TEntity>(IEnumerable<TEntity> source) : IQueryRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _items = source as List<TEntity> ?? source.ToList();

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
