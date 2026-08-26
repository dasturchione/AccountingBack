using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.BankParsers;
using Application.Features.CounterpartyBankAccounts;
using Application.Features.OrgBankAccounts;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class BankBranchAssociationTests
{
    [Fact]
    public async Task OrgBankAccountCreate_RejectsBranchFromAnotherBank()
    {
        var command = new RecordingCommandRepository<BankAccount>();
        var service = new OrgBankAccountService(
            new TestUserContext(organizationId: 7),
            null!,
            new InMemoryQueryRepository<BankAccount>(),
            null!,
            command,
            new InMemoryQueryRepository<BankBranch>(
                new BankBranch { Id = 15, BankId = 2, Mfo = "00440", Name = "Branch" }));

        var result = await service.CreateAsync(new OrgBankAccountCreateDto
        {
            BankId = 1,
            BankBranchId = 15,
            AccountNumber = "20208000123456789001",
            CurrencyId = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("OrgBankAccount.BankBranchMismatch", result.Error.Code);
        Assert.Empty(command.Created);
    }

    [Fact]
    public async Task CounterpartyBankAccountCreate_RejectsBranchFromAnotherBank()
    {
        var command = new RecordingCommandRepository<CounterpartyBankAccount>();
        var service = new CounterpartyBankAccountService(
            new TestUserContext(organizationId: 7),
            null!,
            new InMemoryQueryRepository<CounterpartyBankAccount>(),
            command,
            new InMemoryQueryRepository<BankBranch>(
                new BankBranch { Id = 15, BankId = 2, Mfo = "00440", Name = "Branch" }));

        var result = await service.CreateAsync(new CounterpartyBankAccountCreateDto
        {
            CounterpartyId = 9,
            BankId = 1,
            BankBranchId = 15,
            AccountNumber = "20208000987654321001",
            CurrencyId = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("CounterpartyBankAccount.BankBranchMismatch", result.Error.Code);
        Assert.Empty(command.Created);
    }

    [Fact]
    public async Task EnrichAsync_ReturnsBankBranchIdFoundByMfo()
    {
        var service = new BankStatementParserService(
            new TestUserContext(organizationId: 7),
            new InMemoryQueryRepository<BankStatementTemplate>(),
            new InMemoryQueryRepository<Bank>(),
            new InMemoryQueryRepository<BankBranch>(
                new BankBranch
                {
                    Id = 15,
                    BankId = 2,
                    Mfo = "00440",
                    Name = "Uzsanoatqurilishbank head office",
                    StateId = StateIdConst.ACTIVE
                }),
            new InMemoryQueryRepository<BankAccount>(),
            new InMemoryQueryRepository<CounterpartyCard>(),
            new InMemoryQueryRepository<CounterpartyBankAccount>(),
            new QueryBuilder(new NullQueryBuilderResolver()),
            null!);
        var export = new BankExportDto
        {
            Accounts =
            [
                new AccountStatementDto
                {
                    BankMfo = "00440",
                    AccountNumber = "20208000123456789001"
                }
            ]
        };

        var result = await service.EnrichAsync(export);

        Assert.True(result.IsSuccess);
        var account = Assert.Single(result.Value.Accounts);
        Assert.Equal(15, account.BankBranchId);
        Assert.Equal(2, account.BankId);
    }

    [Fact]
    public async Task EnrichAsync_FindsBranchAfterLegacyBankAccountSuppliesMfo()
    {
        var bank = new Bank { Id = 2, Code = "SQB", Name = "SQB", Mfo = "00440" };
        var service = new BankStatementParserService(
            new TestUserContext(organizationId: 7),
            new InMemoryQueryRepository<BankStatementTemplate>(),
            new InMemoryQueryRepository<Bank>(),
            new InMemoryQueryRepository<BankBranch>(
                new BankBranch
                {
                    Id = 15,
                    BankId = 2,
                    Mfo = "00440",
                    Name = "Uzsanoatqurilishbank head office",
                    StateId = StateIdConst.ACTIVE
                }),
            new InMemoryQueryRepository<BankAccount>(
                new BankAccount
                {
                    Id = 20,
                    OrganizationId = 7,
                    BankId = 2,
                    Bank = bank,
                    AccountNumber = "20208000123456789001",
                    StateId = StateIdConst.ACTIVE
                }),
            new InMemoryQueryRepository<CounterpartyCard>(),
            new InMemoryQueryRepository<CounterpartyBankAccount>(),
            new QueryBuilder(new NullQueryBuilderResolver()),
            null!);
        var export = new BankExportDto
        {
            Accounts =
            [
                new AccountStatementDto { AccountNumber = "20208000123456789001" }
            ]
        };

        var result = await service.EnrichAsync(export);

        Assert.True(result.IsSuccess);
        var account = Assert.Single(result.Value.Accounts);
        Assert.Equal("00440", account.BankMfo);
        Assert.Equal(15, account.BankBranchId);
    }

    private sealed class InMemoryQueryRepository<TEntity>(params TEntity[] entities)
        : IQueryRepository<TEntity> where TEntity : class
    {
        private readonly List<TEntity> _entities = [.. entities];

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().FirstOrDefault(specification.Criteria));

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .FirstOrDefault(specification.ResultCriteria));

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .Where(specification.ResultCriteria)
                .ToList());

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
    {
        public List<TEntity> Created { get; } = [];

        public Task CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            Created.Add(entity);
            return Task.CompletedTask;
        }

        public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            Created.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => throw new NotSupportedException();
        public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
        public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class NullQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => throw new NotSupportedException();
        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }

    private sealed class TestUserContext(int organizationId) : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => [organizationId];
        public int? BranchId => null;
    }
}
