using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.TrialBalance;

public class TrialBalanceService : ITrialBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<AccountingPeriod> _periodQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly ITrialBalanceReadRepository _readRepository;

    public TrialBalanceService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<AccountingPeriod> periodQuery,
        IQueryRepository<Currency> currencyQuery,
        ITrialBalanceReadRepository readRepository)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _periodQuery = periodQuery;
        _currencyQuery = currencyQuery;
        _readRepository = readRepository;
    }

    public async Task<Result<TrialBalanceDto>> GetAsync(TrialBalanceFilter filter, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(filter, ct);
        if (!validation.IsSuccess)
            return Result.Failure<TrialBalanceDto>(validation.Error);

        var readResult = await _readRepository.GetAsync(new TrialBalanceReadRequest
        {
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId
        }, ct);

        var items = readResult.Rows
            .Select(MapRow)
            .Where(x => filter.IncludeZeroBalance || HasAnyBalance(x))
            .ToList();

        return new TrialBalanceDto
        {
            PeriodId = filter.PeriodId,
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId,
            IncludeZeroBalance = filter.IncludeZeroBalance,
            OpeningDebitTotal = items.Sum(x => x.OpeningDebit),
            OpeningCreditTotal = items.Sum(x => x.OpeningCredit),
            PeriodDebitTotal = items.Sum(x => x.PeriodDebit),
            PeriodCreditTotal = items.Sum(x => x.PeriodCredit),
            ClosingDebitTotal = items.Sum(x => x.ClosingDebit),
            ClosingCreditTotal = items.Sum(x => x.ClosingCredit),
            Items = items
        };
    }

    private async Task<Result<TrialBalanceValidationState>> ValidateAsync(TrialBalanceFilter filter, CancellationToken ct)
    {
        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateFrom.Value.Date > filter.DateTo.Value.Date)
            return Result.Failure<TrialBalanceValidationState>(TrialBalanceErrors.InvalidDateRange(_userContext.LanguageId));

        if (filter.CurrencyId.HasValue)
        {
            var currencyExists = await _currencyQuery.AnyAsync(
                x => x.Id == filter.CurrencyId.Value && x.StateId == StateIdConst.ACTIVE,
                ct);

            if (!currencyExists)
                return Result.Failure<TrialBalanceValidationState>(TrialBalanceErrors.CurrencyNotFound(filter.CurrencyId.Value, _userContext.LanguageId));
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
                return Result.Failure<TrialBalanceValidationState>(TrialBalanceErrors.PeriodNotFound(filter.PeriodId.Value, _userContext.LanguageId));

            var periodStart = period.StartDate.ToDateTime(TimeOnly.MinValue);
            var periodEnd = period.EndDate.ToDateTime(TimeOnly.MaxValue);

            dateFrom ??= periodStart;
            dateTo ??= periodEnd;

            if (dateFrom.Value < periodStart || dateTo.Value > periodEnd)
                return Result.Failure<TrialBalanceValidationState>(TrialBalanceErrors.DateRangeOutsidePeriod(period.Id, _userContext.LanguageId));
        }

        return new TrialBalanceValidationState
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };
    }

    private static TrialBalanceItemDto MapRow(TrialBalanceReadRow row)
    {
        var openingNet = row.OpeningDebitTurnover - row.OpeningCreditTurnover;
        var closingNet = openingNet + row.PeriodDebitTurnover - row.PeriodCreditTurnover;

        return new TrialBalanceItemDto
        {
            AccountId = row.AccountId,
            AccountCode = row.AccountCode,
            AccountNumber = row.AccountNumber,
            AccountName = row.AccountName,
            OpeningDebit = openingNet > 0m ? openingNet : 0m,
            OpeningCredit = openingNet < 0m ? decimal.Abs(openingNet) : 0m,
            PeriodDebit = row.PeriodDebitTurnover,
            PeriodCredit = row.PeriodCreditTurnover,
            ClosingDebit = closingNet > 0m ? closingNet : 0m,
            ClosingCredit = closingNet < 0m ? decimal.Abs(closingNet) : 0m
        };
    }

    private static bool HasAnyBalance(TrialBalanceItemDto item) =>
        item.OpeningDebit != 0m ||
        item.OpeningCredit != 0m ||
        item.PeriodDebit != 0m ||
        item.PeriodCredit != 0m ||
        item.ClosingDebit != 0m ||
        item.ClosingCredit != 0m;

    private sealed class TrialBalanceValidationState
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
