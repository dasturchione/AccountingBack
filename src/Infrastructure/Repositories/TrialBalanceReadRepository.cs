using Application.Features.TrialBalance;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TrialBalanceReadRepository : ITrialBalanceReadRepository
{
    private readonly AppDbContext _context;

    public TrialBalanceReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TrialBalanceReadResult> GetAsync(TrialBalanceReadRequest request, CancellationToken ct = default)
    {
        var accounts = await _context.ChartAccounts
            .AsNoTracking()
            .Where(x => x.StateId == SharedKernel.Constants.StateIdConst.ACTIVE)
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Id)
            .Select(x => new TrialBalanceReadRow
            {
                AccountId = x.Id,
                AccountCode = x.Code!,
                AccountNumber = x.Number,
                AccountName = x.Name,
                AccountTypeId = x.AccountTypeId
            })
            .ToListAsync(ct);

        var entryQuery = ApplyCommonFilters(
            _context.AccountingRegisterEntries.AsNoTracking(),
            request);

        var openingDebit = request.DateFrom.HasValue
            ? await LoadAccountSumsAsync(
                entryQuery.Where(x => x.DocDate < RegisterDateRange.InclusiveStart(request.DateFrom.Value) && x.DebitAccountId.HasValue),
                x => x.DebitAccountId,
                ct)
            : new Dictionary<int, decimal>();

        var openingCredit = request.DateFrom.HasValue
            ? await LoadAccountSumsAsync(
                entryQuery.Where(x => x.DocDate < RegisterDateRange.InclusiveStart(request.DateFrom.Value) && x.CreditAccountId.HasValue),
                x => x.CreditAccountId,
                ct)
            : new Dictionary<int, decimal>();

        var periodQuery = ApplyPeriodFilters(entryQuery, request);

        var periodDebit = await LoadAccountSumsAsync(
            periodQuery.Where(x => x.DebitAccountId.HasValue),
            x => x.DebitAccountId,
            ct);

        var periodCredit = await LoadAccountSumsAsync(
            periodQuery.Where(x => x.CreditAccountId.HasValue),
            x => x.CreditAccountId,
            ct);

        foreach (var account in accounts)
        {
            if (openingDebit.TryGetValue(account.AccountId, out var openingDebitAmount))
                account.OpeningDebitTurnover = openingDebitAmount;

            if (openingCredit.TryGetValue(account.AccountId, out var openingCreditAmount))
                account.OpeningCreditTurnover = openingCreditAmount;

            if (periodDebit.TryGetValue(account.AccountId, out var periodDebitAmount))
                account.PeriodDebitTurnover = periodDebitAmount;

            if (periodCredit.TryGetValue(account.AccountId, out var periodCreditAmount))
                account.PeriodCreditTurnover = periodCreditAmount;
        }

        return new TrialBalanceReadResult
        {
            Rows = accounts
        };
    }

    private static IQueryable<AccountingRegisterEntry> ApplyCommonFilters(
        IQueryable<AccountingRegisterEntry> query,
        TrialBalanceReadRequest request)
    {
        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId.Value);

        return query;
    }

    private static IQueryable<AccountingRegisterEntry> ApplyPeriodFilters(
        IQueryable<AccountingRegisterEntry> query,
        TrialBalanceReadRequest request)
    {
        if (request.DateFrom.HasValue)
            query = query.Where(x => x.DocDate >= RegisterDateRange.InclusiveStart(request.DateFrom.Value));

        if (request.DateTo.HasValue)
            query = query.Where(x => x.DocDate < RegisterDateRange.ExclusiveEnd(request.DateTo.Value));

        return query;
    }

    private static async Task<Dictionary<int, decimal>> LoadAccountSumsAsync(
        IQueryable<AccountingRegisterEntry> query,
        System.Linq.Expressions.Expression<Func<AccountingRegisterEntry, int?>> accountSelector,
        CancellationToken ct)
    {
        var rows = await query
            .GroupBy(accountSelector)
            .Select(x => new AccountAmountRow
            {
                AccountId = x.Key,
                Amount = x.Sum(y => y.Amount)
            })
            .Where(x => x.AccountId.HasValue)
            .ToListAsync(ct);

        return rows.ToDictionary(x => x.AccountId!.Value, x => x.Amount);
    }

    private sealed class AccountAmountRow
    {
        public int? AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
