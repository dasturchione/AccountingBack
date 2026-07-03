using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AccountingReports;
using Application.Features.Ledger;
using Application.Features.TrialBalance;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class AccountingReportServiceTests
{
    [Fact]
    public async Task GetIncomeStatementAsync_ShouldCalculateTotals()
    {
        var fixture = AccountingReportFixture.Create();
        fixture.TrialBalanceRepository.Result.Rows.AddRange(
        [
            new TrialBalanceReadRow { AccountId = 1, AccountCode = "9020", AccountName = "Revenue", PeriodCreditTurnover = 1000m },
            new TrialBalanceReadRow { AccountId = 2, AccountCode = "9120", AccountName = "Cost", PeriodDebitTurnover = 400m },
            new TrialBalanceReadRow { AccountId = 3, AccountCode = "9420", AccountName = "Expense", PeriodDebitTurnover = 150m }
        ]);

        var result = await fixture.Service.GetIncomeStatementAsync(new IncomeStatementFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(1000m, result.Value.RevenueTotal);
        Assert.Equal(400m, result.Value.CostOfSalesTotal);
        Assert.Equal(150m, result.Value.OperatingExpenseTotal);
        Assert.Equal(600m, result.Value.GrossProfit);
        Assert.Equal(450m, result.Value.NetProfit);
    }

    [Fact]
    public async Task GetBalanceSheetAsync_ShouldIncludeCurrentPeriodResultInEquity()
    {
        var fixture = AccountingReportFixture.Create();
        fixture.TrialBalanceRepository.Result.Rows.AddRange(
        [
            new TrialBalanceReadRow
            {
                AccountId = 10,
                AccountCode = "5110",
                AccountName = "Bank",
                AccountTypeId = 1,
                OpeningDebitTurnover = 300m
            },
            new TrialBalanceReadRow
            {
                AccountId = 20,
                AccountCode = "6010",
                AccountName = "Supplier",
                AccountTypeId = 2,
                OpeningCreditTurnover = 120m
            },
            new TrialBalanceReadRow
            {
                AccountId = 30,
                AccountCode = "9020",
                AccountName = "Revenue",
                AccountTypeId = 3,
                PeriodCreditTurnover = 500m
            },
            new TrialBalanceReadRow
            {
                AccountId = 40,
                AccountCode = "9120",
                AccountName = "Cost",
                AccountTypeId = 1,
                PeriodDebitTurnover = 200m
            }
        ]);

        var result = await fixture.Service.GetBalanceSheetAsync(new BalanceSheetFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(300m, result.Value.TotalAssets);
        Assert.Equal(120m, result.Value.TotalLiabilities);
        Assert.Equal(300m, result.Value.TotalEquity);
        Assert.Contains(result.Value.Sections.Single(x => x.Code == "EQUITY").Rows, x => x.AccountCode == "CURRENT_RESULT" && x.Balance == 300m);
    }

    [Fact]
    public async Task GetAccountCardAsync_ShouldReuseLedgerService()
    {
        var fixture = AccountingReportFixture.Create();
        fixture.LedgerService.Response = Result.Success(new LedgerDto
        {
            AccountId = 101,
            AccountCode = "5110",
            AccountName = "Bank",
            OpeningBalance = 10m,
            ClosingBalance = 30m,
            TotalDebit = 25m,
            TotalCredit = 5m,
            Page = 1,
            PageSize = 50,
            TotalCount = 1,
            TotalPages = 1,
            Transactions =
            [
                new LedgerTransactionDto
                {
                    Id = 1,
                    PostingDate = new DateTime(2026, 7, 3),
                    DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                    DocumentType = "Purchase",
                    Debit = 25m,
                    Credit = 5m,
                    RunningBalance = 30m,
                    CurrencyId = 1,
                    Currency = "UZS",
                    OrganizationId = 8,
                    Organization = "Org"
                }
            ]
        });

        var result = await fixture.Service.GetAccountCardAsync(new AccountCardFilter { AccountId = 101 });

        Assert.True(result.IsSuccess);
        Assert.Equal("5110", result.Value.AccountCode);
        Assert.Single(result.Value.Transactions);
        Assert.Equal(30m, result.Value.Transactions[0].RunningBalance);
    }
}

file sealed class AccountingReportFixture
{
    public required AccountingReportService Service { get; init; }
    public required FakeTrialBalanceReadRepository TrialBalanceRepository { get; init; }
    public required FakeLedgerService LedgerService { get; init; }

    public static AccountingReportFixture Create()
    {
        var trialBalanceRepository = new FakeTrialBalanceReadRepository();
        var ledgerService = new FakeLedgerService();

        var service = new AccountingReportService(
            new AccountingReportUserContext(),
            new AccountingReportQueryBuilder(),
            new AccountingReportQueryRepository<AccountingPeriod>([]),
            new AccountingReportQueryRepository<Currency>([]),
            new AccountingReportQueryRepository<ChartAccount>([]),
            trialBalanceRepository,
            ledgerService,
            new FakeAccountingReportReadRepository());

        return new AccountingReportFixture
        {
            Service = service,
            TrialBalanceRepository = trialBalanceRepository,
            LedgerService = ledgerService
        };
    }
}

file sealed class AccountingReportUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => LanguageIdConst.UZ;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeTrialBalanceReadRepository : ITrialBalanceReadRepository
{
    public TrialBalanceReadResult Result { get; } = new();

    public Task<TrialBalanceReadResult> GetAsync(TrialBalanceReadRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result);
}

file sealed class FakeLedgerService : ILedgerService
{
    public Result<LedgerDto> Response { get; set; } = SharedKernel.Results.Result.Success(new LedgerDto());

    public Task<Result<LedgerDto>> GetAsync(LedgerFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Response);
}

file sealed class FakeAccountingReportReadRepository : IAccountingReportReadRepository
{
    public Task<CashFlowReadResult> GetCashFlowAsync(CashFlowReadRequest request, CancellationToken ct = default) =>
        Task.FromResult(new CashFlowReadResult());

    public Task<JournalReadResult> GetJournalAsync(JournalReadRequest request, CancellationToken ct = default) =>
        Task.FromResult(new JournalReadResult());
}

file sealed class AccountingReportQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public AccountingReportQueryRepository(List<TEntity> data)
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
        Task.FromResult(new PagedList<TEntity>([], 0));

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TResult>([], 0));
}

file sealed class AccountingReportQueryBuilder : IQueryBuilder
{
    private static readonly AccountingReportQueryBuilderResolver Resolver = new();

    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = Resolver });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true };

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default! };

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, Skip = 0, Take = 50 };

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default!, Skip = 0, Take = 50 };
}

file sealed class AccountingReportQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new AccountingReportProjectionBuilder<TEntity, TResult>();
}

file sealed class AccountingReportProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
