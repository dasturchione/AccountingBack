using Application.Features.CashDocuments;
using Application.Features.Register;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;

namespace Infrastructure.Repositories;

public class CashBookReadRepository : ICashBookReadRepository
{
    private readonly AppDbContext _context;

    public CashBookReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CashBookReadResult> GetAsync(CashBookReadRequest request, CancellationToken ct = default)
    {
        var filteredEntries = BuildQuery(request, applyDateRange: true);
        var orderedEntries = filteredEntries
            .OrderBy(x => x.DocDate)
            .ThenBy(x => x.MoneyRegisterEntryId);

        var totalCount = await filteredEntries.CountAsync(ct);
        var pageItems = await orderedEntries
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var openingBalance = request.DateFrom.HasValue
            ? await SumNetAsync(
                BuildQuery(request, applyDateRange: false)
                    .Where(x => x.DocDate < request.DateFrom.Value),
                ct)
            : await GetOpeningBalanceAsync(request.CashBoxId, ct);

        if (request.DateFrom.HasValue)
            openingBalance += await GetOpeningBalanceAsync(request.CashBoxId, ct);

        var pageOpeningBalance = openingBalance;
        if (pageItems.Count > 0)
        {
            var firstItem = pageItems[0];
            pageOpeningBalance += await SumNetAsync(
                filteredEntries.Where(x =>
                    x.DocDate < firstItem.DocDate ||
                    (x.DocDate == firstItem.DocDate && x.MoneyRegisterEntryId < firstItem.MoneyRegisterEntryId)),
                ct);
        }

        var totalReceipt = await SumAsync(
            filteredEntries.Select(x => (decimal?)(x.OperationTypeId == OperationTypeIdConst.IN ? x.Amount : 0m)),
            ct);
        var totalPayment = await SumAsync(
            filteredEntries.Select(x => (decimal?)(x.OperationTypeId == OperationTypeIdConst.OUT ? x.Amount : 0m)),
            ct);

        return new CashBookReadResult
        {
            OpeningBalance = openingBalance,
            ClosingBalance = openingBalance + totalReceipt - totalPayment,
            PageOpeningBalance = pageOpeningBalance,
            TotalReceipt = totalReceipt,
            TotalPayment = totalPayment,
            TotalCount = totalCount,
            Entries = pageItems.Select(MapEntry).ToList()
        };
    }

    private IQueryable<CashBookQueryRow> BuildQuery(CashBookReadRequest request, bool applyDateRange)
    {
        var query =
            from entry in _context.MoneyRegisterBalances.AsNoTracking()
            join document in _context.CashOperations.AsNoTracking() on entry.DocumentId equals document.Id
            where entry.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION
                  && entry.SourceId == request.CashBoxId
                  && EF.Functions.Like(entry.SourceType, $"{RegisterDefaultsConst.CashOperation}%")
            select new CashBookQueryRow
            {
                MoneyRegisterEntryId = entry.Id,
                CashOperationId = document.Id,
                DocDate = entry.DocDate,
                DocNumber = document.DocNumber,
                PaymentPurposeId = document.PaymentPurposeId,
                PaymentPurposeName = document.PaymentPurpose.Name,
                CounterpartyId = document.CounterpartyId,
                CounterpartyName = document.Counterparty != null ? document.Counterparty.ShortName : null,
                Comment = document.Comment,
                CurrencyId = document.CurrencyId,
                CurrencyName = document.Currency.Code,
                OperationTypeId = entry.OperationTypeId,
                Amount = entry.Amount
            };

        if (applyDateRange && request.DateFrom.HasValue)
            query = query.Where(x => x.DocDate >= request.DateFrom.Value);

        if (applyDateRange && request.DateTo.HasValue)
            query = query.Where(x => x.DocDate <= request.DateTo.Value);

        return query;
    }

    private async Task<decimal> GetOpeningBalanceAsync(int cashBoxId, CancellationToken ct) =>
        await _context.CashBoxes
            .AsNoTracking()
            .Where(x => x.Id == cashBoxId)
            .Select(x => (decimal?)x.OpeningBalance)
            .FirstOrDefaultAsync(ct) ?? 0m;

    private static CashBookReadEntry MapEntry(CashBookQueryRow row) =>
        new()
        {
            MoneyRegisterEntryId = row.MoneyRegisterEntryId,
            CashOperationId = row.CashOperationId,
            DocDate = row.DocDate,
            DocNumber = row.DocNumber,
            DocumentKind = row.OperationTypeId == OperationTypeIdConst.IN ? "PKO" : "RKO",
            PaymentPurposeId = row.PaymentPurposeId,
            PaymentPurposeName = row.PaymentPurposeName,
            CounterpartyId = row.CounterpartyId,
            CounterpartyName = row.CounterpartyName,
            Comment = row.Comment,
            CurrencyId = row.CurrencyId,
            CurrencyName = row.CurrencyName,
            Receipt = row.OperationTypeId == OperationTypeIdConst.IN ? row.Amount : 0m,
            Payment = row.OperationTypeId == OperationTypeIdConst.OUT ? row.Amount : 0m
        };

    private static async Task<decimal> SumNetAsync(IQueryable<CashBookQueryRow> query, CancellationToken ct) =>
        await SumAsync(query.Select(x => (decimal?)(x.OperationTypeId == OperationTypeIdConst.IN ? x.Amount : -x.Amount)), ct);

    private static async Task<decimal> SumAsync(IQueryable<decimal?> query, CancellationToken ct) =>
        (await query.SumAsync(ct)) ?? 0m;

    private sealed class CashBookQueryRow
    {
        public long MoneyRegisterEntryId { get; set; }
        public long CashOperationId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNumber { get; set; } = null!;
        public short PaymentPurposeId { get; set; }
        public string PaymentPurposeName { get; set; } = null!;
        public int? CounterpartyId { get; set; }
        public string? CounterpartyName { get; set; }
        public string? Comment { get; set; }
        public short CurrencyId { get; set; }
        public string CurrencyName { get; set; } = null!;
        public short OperationTypeId { get; set; }
        public decimal Amount { get; set; }
    }
}
