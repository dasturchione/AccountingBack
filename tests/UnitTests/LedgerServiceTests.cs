using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Ledger;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class LedgerServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldRejectMissingAccount()
    {
        var fixture = LedgerFixture.Create();

        var result = await fixture.Service.GetAsync(new LedgerFilter
        {
            AccountId = 0,
            Page = 1,
            PageSize = 50
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Ledger.AccountRequired", result.Error.Code);
        Assert.Null(fixture.ReadRepository.LastRequest);
    }

    [Fact]
    public async Task GetAsync_ShouldRejectInvalidDateRange()
    {
        var fixture = LedgerFixture.Create(accounts:
        [
            new ChartAccount
            {
                Id = 2910,
                Code = "2910",
                Name = "Inventory",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Today
            }
        ]);

        var result = await fixture.Service.GetAsync(new LedgerFilter
        {
            AccountId = 2910,
            DateFrom = new DateTime(2026, 7, 10),
            DateTo = new DateTime(2026, 7, 1),
            Page = 1,
            PageSize = 50
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Ledger.InvalidDateRange", result.Error.Code);
        Assert.Null(fixture.ReadRepository.LastRequest);
    }

    [Fact]
    public async Task GetAsync_ShouldRejectUnknownCurrency()
    {
        var fixture = LedgerFixture.Create(
            accounts:
            [
                new ChartAccount
                {
                    Id = 2910,
                    Code = "2910",
                    Name = "Inventory",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Today
                }
            ],
            currencies: []);

        var result = await fixture.Service.GetAsync(new LedgerFilter
        {
            AccountId = 2910,
            CurrencyId = 999,
            Page = 1,
            PageSize = 50
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Ledger.CurrencyNotFound", result.Error.Code);
        Assert.Null(fixture.ReadRepository.LastRequest);
    }

    [Fact]
    public async Task GetAsync_ShouldApplyPeriodBoundaries_WhenPeriodIsSelected()
    {
        var fixture = LedgerFixture.Create(
            accounts:
            [
                new ChartAccount
                {
                    Id = 2910,
                    Code = "2910",
                    Name = "Inventory",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Today
                }
            ],
            periods:
            [
                new AccountingPeriod
                {
                    Id = 202607,
                    OrganizationId = 8,
                    Year = 2026,
                    Month = 7,
                    StartDate = new DateOnly(2026, 7, 1),
                    EndDate = new DateOnly(2026, 7, 31),
                    CreatedDate = DateTime.Today
                }
            ]);

        fixture.ReadRepository.Result = new LedgerReadResult();

        var result = await fixture.Service.GetAsync(new LedgerFilter
        {
            AccountId = 2910,
            PeriodId = 202607,
            Page = 1,
            PageSize = 50
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(fixture.ReadRepository.LastRequest);
        Assert.Equal(new DateTime(2026, 7, 1, 0, 0, 0), fixture.ReadRepository.LastRequest!.DateFrom);
        Assert.Equal(new DateTime(2026, 7, 31, 23, 59, 59, 999).AddTicks(9999), fixture.ReadRepository.LastRequest.DateTo);
    }

    [Fact]
    public async Task GetAsync_ShouldCalculateRunningBalance_FromPageOpeningBalance()
    {
        var fixture = LedgerFixture.Create(
            accounts:
            [
                new ChartAccount
                {
                    Id = 2910,
                    Code = "2910",
                    Name = "Inventory",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Today
                }
            ]);

        fixture.ReadRepository.Result = new LedgerReadResult
        {
            OpeningBalance = 100m,
            ClosingBalance = 130m,
            PageOpeningBalance = 90m,
            TotalDebit = 50m,
            TotalCredit = 10m,
            TotalCount = 2,
            Transactions =
            [
                new LedgerReadTransaction
                {
                    Id = 1,
                    PostingDate = new DateTime(2026, 7, 3),
                    Debit = 50m,
                    Credit = 0m,
                    CurrencyId = 1,
                    Currency = "UZS",
                    OrganizationId = 8,
                    Organization = "Org"
                },
                new LedgerReadTransaction
                {
                    Id = 2,
                    PostingDate = new DateTime(2026, 7, 4),
                    Debit = 0m,
                    Credit = 10m,
                    CurrencyId = 1,
                    Currency = "UZS",
                    OrganizationId = 8,
                    Organization = "Org"
                }
            ]
        };

        var result = await fixture.Service.GetAsync(new LedgerFilter
        {
            AccountId = 2910,
            Page = 2,
            PageSize = 2
        });

        Assert.True(result.IsSuccess);
        Assert.Equal([140m, 130m], result.Value.Transactions.Select(x => x.RunningBalance).ToArray());
        Assert.Equal(1, result.Value.TotalPages);
        Assert.True(result.Value.HasPreviousPage);
        Assert.False(result.Value.HasNextPage);
    }
}

file sealed class LedgerFixture
{
    public required LedgerService Service { get; init; }
    public required LedgerReadRepositoryStub ReadRepository { get; init; }

    public static LedgerFixture Create(
        IEnumerable<ChartAccount>? accounts = null,
        IEnumerable<AccountingPeriod>? periods = null,
        IEnumerable<Currency>? currencies = null)
    {
        var readRepository = new LedgerReadRepositoryStub();

        var service = new LedgerService(
            new LedgerUserContext(),
            new LedgerQueryBuilder(),
            new LedgerQueryRepository<ChartAccount>((accounts ?? []).ToList()),
            new LedgerQueryRepository<AccountingPeriod>((periods ?? []).ToList()),
            new LedgerQueryRepository<Currency>((currencies ??
            [
                new Currency
                {
                    Id = 1,
                    Code = "UZS",
                    Name = "Uzbek Sum",
                    StateId = StateIdConst.ACTIVE
                }
            ]).ToList()),
            readRepository);

        return new LedgerFixture
        {
            Service = service,
            ReadRepository = readRepository
        };
    }
}

file sealed class LedgerReadRepositoryStub : ILedgerReadRepository
{
    public LedgerReadRequest? LastRequest { get; private set; }
    public LedgerReadResult Result { get; set; } = new();

    public Task<LedgerReadResult> GetAsync(LedgerReadRequest request, CancellationToken ct = default)
    {
        LastRequest = request;
        return Task.FromResult(Result);
    }
}

file sealed class LedgerUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => LanguageIdConst.UZ;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class LedgerQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public LedgerQueryRepository(List<TEntity> data)
    {
        _data = data;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Any(predicate));

    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).FirstOrDefault());

    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).FirstOrDefault());

    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).ToList());

    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).ToList());

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

file sealed class LedgerQueryBuilder : IQueryBuilder
{
    private static readonly LedgerQueryBuilderResolver Resolver = new();

    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = Resolver });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true };

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default! };

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        throw new NotSupportedException();

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        throw new NotSupportedException();
}

file sealed class LedgerQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new LedgerProjectionBuilder<TEntity, TResult>();
}

file sealed class LedgerProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
