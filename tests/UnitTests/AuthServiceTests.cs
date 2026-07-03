using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Auth;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldIgnoreQueryFiltersForAuthenticationQueries()
    {
        var role = new Role
        {
            Id = 10,
            FullName = "Accountant",
            ShortName = "acc",
            HasGlobalAccess = false,
            StateId = StateIdConst.ACTIVE,
            State = new State { Id = StateIdConst.ACTIVE, ShortName = "Active" }
        };

        var user = new User
        {
            Id = 4,
            UserName = "sardorbek_hafizov",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            PhoneNumber = "+998900000000",
            FirstName = "Sardorbek",
            LastName = "Hafizov",
            RoleId = role.Id,
            Role = role,
            StateId = StateIdConst.ACTIVE,
            State = new State { Id = StateIdConst.ACTIVE, ShortName = "Active" },
            CreatedDate = DateTime.UtcNow
        };

        var organization = new Organization
        {
            Id = 8,
            ShortName = "Org 8",
            FullName = "Organization 8",
            Inn = "123",
            RegionId = 1,
            IsParent = false,
            StateId = StateIdConst.ACTIVE,
            SetupStatus = "DONE",
            CreatedDate = DateTime.UtcNow
        };

        var userOrganizations = new List<UserOrganization>
        {
            new()
            {
                UserId = user.Id,
                OrganizationId = organization.Id,
                Organization = organization,
                RoleId = role.Id,
                Role = role,
                StateId = StateIdConst.ACTIVE,
                State = new State { Id = StateIdConst.ACTIVE, ShortName = "Active" },
                IsDefault = true,
                CreatedDate = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow,
                User = user
            }
        };

        var userRepo = new FakeQueryRepository<User>([user]);
        var userOrgRepo = new FakeQueryRepository<UserOrganization>(userOrganizations);
        var roleModuleRepo = new FakeQueryRepository<RoleModule>([]);
        var moduleRepo = new FakeQueryRepository<Module>([]);
        var commandRepo = new FakeCommandRepository<User>();
        var service = new AuthService(
            new FakeUserContext(),
            new FakeQueryBuilder(),
            new FakeTokenProvider(),
            new FakePasswordHasher(verifyResult: true),
            userRepo,
            commandRepo,
            roleModuleRepo,
            userOrgRepo,
            moduleRepo);

        var result = await service.LoginAsync(new LoginDto
        {
            UserName = " sardorbek_hafizov ",
            Password = "secret123"
        });

        Assert.True(result.IsSuccess);
        Assert.True(userRepo.LastEntitySpecificationIgnoreQueryFilters);
        Assert.True(userOrgRepo.LastProjectionSpecificationIgnoreQueryFilters);
        Assert.Single(result.Value.User.Organizations);
        Assert.Equal(organization.Id, result.Value.User.Organizations[0].OrganizationId);
    }

    [Fact]
    public async Task SuperAdminLoginAsync_ShouldReturnUnauthorized_WhenPasswordHasherThrowsFormatException()
    {
        var role = new Role
        {
            Id = 5,
            FullName = "Super Admin",
            ShortName = "super",
            HasGlobalAccess = true,
            StateId = StateIdConst.ACTIVE,
            State = new State { Id = StateIdConst.ACTIVE, ShortName = "Active" }
        };

        var user = new User
        {
            Id = 15,
            UserName = "superadmin",
            PasswordHash = "invalid",
            PasswordSalt = "invalid",
            PhoneNumber = "+998900000001",
            FirstName = "Super",
            LastName = "Admin",
            RoleId = role.Id,
            Role = role,
            StateId = StateIdConst.ACTIVE,
            State = new State { Id = StateIdConst.ACTIVE, ShortName = "Active" },
            CreatedDate = DateTime.UtcNow
        };

        var service = new AuthService(
            new FakeUserContext(),
            new FakeQueryBuilder(),
            new FakeTokenProvider(),
            new ThrowingPasswordHasher(),
            new FakeQueryRepository<User>([user]),
            new FakeCommandRepository<User>(),
            new FakeQueryRepository<RoleModule>([]),
            new FakeQueryRepository<UserOrganization>([]),
            new FakeQueryRepository<Module>([]));

        var result = await service.SuperAdminLoginAsync(new LoginDto
        {
            UserName = "superadmin",
            Password = "any-password"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    private sealed class FakeQueryRepository<TEntity>(List<TEntity> items) : IQueryRepository<TEntity> where TEntity : class
    {
        public bool LastEntitySpecificationIgnoreQueryFilters { get; private set; }
        public bool LastProjectionSpecificationIgnoreQueryFilters { get; private set; }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            LastEntitySpecificationIgnoreQueryFilters = specification.IgnoreQueryFilters;
            return Task.FromResult(items.AsQueryable().FirstOrDefault(specification.Criteria));
        }

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            LastProjectionSpecificationIgnoreQueryFilters = specification.IgnoreQueryFilters;
            return Task.FromResult(items.AsQueryable().Where(specification.Criteria).Select(specification.Selector).FirstOrDefault());
        }

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            LastEntitySpecificationIgnoreQueryFilters = specification.IgnoreQueryFilters;
            var query = items.AsQueryable().Where(specification.Criteria);
            return Task.FromResult(query.ToList());
        }

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            LastProjectionSpecificationIgnoreQueryFilters = specification.IgnoreQueryFilters;
            var query = items.AsQueryable().Where(specification.Criteria).Select(specification.Selector);
            return Task.FromResult(query.ToList());
        }

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            var query = items.AsQueryable().Where(specification.Criteria);
            var list = query.ToList();
            return Task.FromResult(new PagedList<TEntity>(list, list.Count));
        }

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            var query = items.AsQueryable().Where(specification.Criteria).Select(specification.Selector);
            var list = query.ToList();
            return Task.FromResult(new PagedList<TResult>(list, list.Count));
        }
    }

    private sealed class FakeCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
    {
        public Task CreateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
        public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeQueryBuilder : IQueryBuilder
    {
        public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
            new(new QueryState<TEntity> { Resolver = new FakeQueryBuilderResolver() });

        public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class => throw new NotSupportedException();
        public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class => throw new NotSupportedException();
        public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options) where TEntity : class where TOptions : IPaginationFilter => throw new NotSupportedException();
        public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options) where TEntity : class where TOptions : IPaginationFilter => throw new NotSupportedException();
    }

    private sealed class FakeQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => throw new NotSupportedException();
    }

    private sealed class FakeTokenProvider : ITokenProvider
    {
        public string GenerateAccessToken(User user, int organizationId) => $"token-{user.Id}-{organizationId}";
        public string GenerateRefreshToken() => "refresh";
        public int GetRefreshTokenExpirationDays() => 7;
        public string GetRefreshTokenHash(string refreshToken) => refreshToken;
    }

    private sealed class FakePasswordHasher(bool verifyResult) : IPasswordHasher
    {
        public string GenerateSalt() => "salt";
        public string Hash(string password, string passwordSalt) => "hash";
        public bool Verify(string password, string passwordSalt, string passwordHash) => verifyResult;
    }

    private sealed class ThrowingPasswordHasher : IPasswordHasher
    {
        public string GenerateSalt() => "salt";
        public string Hash(string password, string passwordSalt) => "hash";
        public bool Verify(string password, string passwordSalt, string passwordHash) => throw new FormatException("Invalid hash format.");
    }

    private sealed class FakeUserContext : IUserContext
    {
        public int? Id => null;
        public int? RoleId => null;
        public short? LanguageId => LanguageIdConst.EN;
        public int? OrganizationId => null;
        public List<int> AllowedOrganizationIds => [];
        public int? BranchId => null;
        public bool HasGlobalAccess => false;
    }
}
