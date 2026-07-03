using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.TrialBalance;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public class TrialBalanceServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldRejectInvalidDateRange()
    {
        var fixture = TrialBalanceFixture.Create();

        var result = await fixture.Service.GetAsync(new TrialBalanceFilter
        {
            DateFrom = new DateTime(2026, 8, 1),
            DateTo = new DateTime(2026, 7, 1)
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("TrialBalance.InvalidDateRange", result.Error.Code);
        Assert.Null(fixture.ReadRepository.LastRequest);
    }

    [Fact]
    public async Task GetAsync_ShouldRejectUnknownCurrency()
    {
        var fixture = TrialBalanceFixture.Create(currencies: []);

        var result = await fixture.Service.GetAsync(new TrialBalanceFilter
        {
            CurrencyId = 999
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("TrialBalance.CurrencyNotFound", result.Error.Code);
        Assert.Null(fixture.ReadRepository.LastRequest);
    }

    [Fact]
    public async Task GetAsync_ShouldApplyPeriodBoundaries_WhenPeriodSelected()
    {
        var fixture = TrialBalanceFixture.Create(periods:
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

        fixture.ReadRepository.Result = new TrialBalanceReadResult();

        var result = await fixture.Service.GetAsync(new TrialBalanceFilter
        {
            PeriodId = 202607
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(fixture.ReadRepository.LastRequest);
        Assert.Equal(new DateTime(2026, 7, 1, 0, 0, 0), fixture.ReadRepository.LastRequest!.DateFrom);
        Assert.Equal(new DateTime(2026, 7, 31, 23, 59, 59, 999).AddTicks(9999), fixture.ReadRepository.LastRequest.DateTo);
    }

    [Fact]
    public async Task GetAsync_ShouldComputeBalances_AndExcludeZeroAccounts_ByDefault()
    {
        var fixture = TrialBalanceFixture.Create();
        fixture.ReadRepository.Result = new TrialBalanceReadResult
        {
            Rows =
            [
                new TrialBalanceReadRow
                {
                    AccountId = 2910,
                    AccountCode = "2910",
                    AccountName = "Inventory",
                    OpeningDebitTurnover = 100m,
                    OpeningCreditTurnover = 20m,
                    PeriodDebitTurnover = 50m,
                    PeriodCreditTurnover = 10m
                },
                new TrialBalanceReadRow
                {
                    AccountId = 6010,
                    AccountCode = "6010",
                    AccountName = "Supplier"
                }
            ]
        };

        var result = await fixture.Service.GetAsync(new TrialBalanceFilter());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        var item = result.Value.Items[0];
        Assert.Equal(80m, item.OpeningDebit);
        Assert.Equal(0m, item.OpeningCredit);
        Assert.Equal(50m, item.PeriodDebit);
        Assert.Equal(10m, item.PeriodCredit);
        Assert.Equal(120m, item.ClosingDebit);
        Assert.Equal(0m, item.ClosingCredit);
        Assert.Equal(80m, result.Value.OpeningDebitTotal);
        Assert.Equal(120m, result.Value.ClosingDebitTotal);
    }

    [Fact]
    public async Task GetAsync_ShouldIncludeZeroAccounts_WhenRequested()
    {
        var fixture = TrialBalanceFixture.Create();
        fixture.ReadRepository.Result = new TrialBalanceReadResult
        {
            Rows =
            [
                new TrialBalanceReadRow
                {
                    AccountId = 6010,
                    AccountCode = "6010",
                    AccountName = "Supplier"
                }
            ]
        };

        var result = await fixture.Service.GetAsync(new TrialBalanceFilter
        {
            IncludeZeroBalance = true
        });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("6010", result.Value.Items[0].AccountCode);
    }
}

file sealed class TrialBalanceFixture
{
    public required TrialBalanceService Service { get; init; }
    public required TrialBalanceReadRepositoryStub ReadRepository { get; init; }

    public static TrialBalanceFixture Create(
        IEnumerable<AccountingPeriod>? periods = null,
        IEnumerable<Currency>? currencies = null)
    {
        var readRepository = new TrialBalanceReadRepositoryStub();

        var service = new TrialBalanceService(
            new TrialBalanceUserContext(),
            new TrialBalanceQueryBuilder(),
            new TrialBalanceQueryRepository<AccountingPeriod>((periods ?? []).ToList()),
            new TrialBalanceQueryRepository<Currency>((currencies ??
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

        return new TrialBalanceFixture
        {
            Service = service,
            ReadRepository = readRepository
        };
    }
}

file sealed class TrialBalanceReadRepositoryStub : ITrialBalanceReadRepository
{
    public TrialBalanceReadRequest? LastRequest { get; private set; }
    public TrialBalanceReadResult Result { get; set; } = new();

    public Task<TrialBalanceReadResult> GetAsync(TrialBalanceReadRequest request, CancellationToken ct = default)
    {
        LastRequest = request;
        return Task.FromResult(Result);
    }
}

file sealed class TrialBalanceUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => LanguageIdConst.UZ;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class TrialBalanceQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public TrialBalanceQueryRepository(List<TEntity> data)
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

file sealed class TrialBalanceQueryBuilder : IQueryBuilder
{
    private static readonly TrialBalanceQueryBuilderResolver Resolver = new();

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

file sealed class TrialBalanceQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new TrialBalanceProjectionBuilder<TEntity, TResult>();
}

file sealed class TrialBalanceProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
