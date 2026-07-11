using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Ledger;

public class LedgerService : ILedgerService
{
    private const int DefaultPageSize = 50;

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly IQueryRepository<AccountingPeriod> _periodQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly ILedgerReadRepository _ledgerReadRepository;

    public LedgerService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<ChartAccount> chartAccountQuery,
        IQueryRepository<AccountingPeriod> periodQuery,
        IQueryRepository<Currency> currencyQuery,
        ILedgerReadRepository ledgerReadRepository)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _chartAccountQuery = chartAccountQuery;
        _periodQuery = periodQuery;
        _currencyQuery = currencyQuery;
        _ledgerReadRepository = ledgerReadRepository;
    }

    public async Task<Result<LedgerDto>> GetAsync(LedgerFilter filter, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(filter, ct);
        if (!validation.IsSuccess)
            return Result.Failure<LedgerDto>(validation.Error);

        var account = validation.Value.Account;
        var dateFrom = validation.Value.DateFrom;
        var dateTo = validation.Value.DateTo;
        var pageSize = filter.PageSize ?? DefaultPageSize;

        var readResult = await _ledgerReadRepository.GetAsync(new LedgerReadRequest
        {
            AccountId = filter.AccountId,
            PeriodId = filter.PeriodId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            CurrencyId = filter.CurrencyId,
            CounterpartyId = filter.CounterpartyId,
            WarehouseId = filter.WarehouseId,
            Page = filter.Page,
            PageSize = pageSize
        }, ct);

        var runningBalance = readResult.PageOpeningBalance;
        var transactions = new List<LedgerTransactionDto>(readResult.Transactions.Count);

        foreach (var item in readResult.Transactions)
        {
            runningBalance += item.Debit - item.Credit;

            transactions.Add(new LedgerTransactionDto
            {
                Id = item.Id,
                PostingDate = item.PostingDate,
                JournalNumber = item.JournalNumber,
                DocumentNumber = item.DocumentNumber,
                DocumentTypeId = item.DocumentTypeId,
                DocumentType = item.DocumentType,
                Reference = item.Reference,
                Description = item.Description,
                Debit = item.Debit,
                Credit = item.Credit,
                RunningBalance = runningBalance,
                CurrencyId = item.CurrencyId,
                Currency = item.Currency,
                OrganizationId = item.OrganizationId,
                Organization = item.Organization,
                CounterpartyId = item.CounterpartyId,
                Counterparty = item.Counterparty,
                WarehouseId = item.WarehouseId,
                Warehouse = item.Warehouse
            });
        }

        var totalPages = readResult.TotalCount > 0
            ? (int)Math.Ceiling(readResult.TotalCount / (double)pageSize)
            : 0;

        return new LedgerDto
        {
            AccountId = account.Id,
            AccountCode = account.Code!,
            AccountName = account.Name,
            PeriodId = filter.PeriodId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            CurrencyId = filter.CurrencyId,
            OpeningBalance = readResult.OpeningBalance,
            ClosingBalance = readResult.ClosingBalance,
            TotalDebit = readResult.TotalDebit,
            TotalCredit = readResult.TotalCredit,
            Page = filter.Page,
            PageSize = pageSize,
            TotalCount = readResult.TotalCount,
            TotalPages = totalPages,
            HasPreviousPage = filter.Page > 1,
            HasNextPage = filter.Page < totalPages,
            Transactions = transactions
        };
    }

    private async Task<Result<LedgerValidationState>> ValidateAsync(LedgerFilter filter, CancellationToken ct)
    {
        if (filter.AccountId <= 0)
            return Result.Failure<LedgerValidationState>(LedgerErrors.AccountRequired(_userContext.LanguageId));

        if (filter.Page <= 0 || filter.PageSize <= 0)
            return Result.Failure<LedgerValidationState>(LedgerErrors.InvalidPagination(_userContext.LanguageId));

        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateFrom.Value.Date > filter.DateTo.Value.Date)
            return Result.Failure<LedgerValidationState>(LedgerErrors.InvalidDateRange(_userContext.LanguageId));

        var accountQuery = _queryBuilder.For<ChartAccount>()
            .Where(x => x.Id == filter.AccountId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var account = await _chartAccountQuery.GetAsync(accountQuery, ct);
        if (account is null)
            return Result.Failure<LedgerValidationState>(LedgerErrors.AccountNotFound(filter.AccountId, _userContext.LanguageId));

        if (filter.CurrencyId.HasValue)
        {
            var currencyExists = await _currencyQuery.AnyAsync(
                x => x.Id == filter.CurrencyId.Value && x.StateId == StateIdConst.ACTIVE,
                ct);

            if (!currencyExists)
                return Result.Failure<LedgerValidationState>(LedgerErrors.CurrencyNotFound(filter.CurrencyId.Value, _userContext.LanguageId));
        }

        DateTime? dateFrom = filter.DateFrom?.Date;
        DateTime? dateTo = filter.DateTo?.Date.AddDays(1).AddTicks(-1);

        if (filter.PeriodId.HasValue)
        {
            var periodQuery = _queryBuilder.For<AccountingPeriod>()
                .Where(x => x.Id == filter.PeriodId.Value)
                .Build();
            var period = await _periodQuery.GetAsync(periodQuery, ct);

            if (period is null)
                return Result.Failure<LedgerValidationState>(LedgerErrors.PeriodNotFound(filter.PeriodId.Value, _userContext.LanguageId));

            var periodStart = period.StartDate.ToDateTime(TimeOnly.MinValue);
            var periodEnd = period.EndDate.ToDateTime(TimeOnly.MaxValue);

            dateFrom ??= periodStart;
            dateTo ??= periodEnd;

            if (dateFrom.Value < periodStart || dateTo.Value > periodEnd)
                return Result.Failure<LedgerValidationState>(LedgerErrors.DateRangeOutsidePeriod(period.Id, _userContext.LanguageId));
        }

        return new LedgerValidationState
        {
            Account = account,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
    }

    private sealed class LedgerValidationState
    {
        public ChartAccount Account { get; set; } = null!;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
