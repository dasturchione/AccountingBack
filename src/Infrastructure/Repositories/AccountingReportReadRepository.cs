using Application.Features.AccountingReports;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class AccountingReportReadRepository : IAccountingReportReadRepository
{
    private readonly AppDbContext _context;

    public AccountingReportReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CashFlowReadResult> GetCashFlowAsync(CashFlowReadRequest request, CancellationToken ct = default)
    {
        var openingBalance = await GetCashBalanceAsync(request.DateFrom, request.CurrencyId, beforeStart: true, ct);
        var periodNet = await GetCashBalanceAsync(request.DateTo, request.CurrencyId, beforeStart: false, ct, request.DateFrom);

        var inflows = BuildCashInflowQuery(request)
            .Select(x => new CashFlowReadRow
            {
                CounterpartAccountCode = x.CounterpartAccountCode ?? UnknownAccountCode,
                CounterpartAccountName = x.CounterpartAccountName ?? UnknownAccountName,
                Inflow = x.Amount,
                Outflow = 0m
            });

        var outflows = BuildCashOutflowQuery(request)
            .Select(x => new CashFlowReadRow
            {
                CounterpartAccountCode = x.CounterpartAccountCode ?? UnknownAccountCode,
                CounterpartAccountName = x.CounterpartAccountName ?? UnknownAccountName,
                Inflow = 0m,
                Outflow = x.Amount
            });

        var rows = await inflows
            .Concat(outflows)
            .GroupBy(x => new { x.CounterpartAccountCode, x.CounterpartAccountName })
            .Select(x => new CashFlowReadRow
            {
                CounterpartAccountCode = x.Key.CounterpartAccountCode,
                CounterpartAccountName = x.Key.CounterpartAccountName,
                Inflow = x.Sum(y => y.Inflow),
                Outflow = x.Sum(y => y.Outflow)
            })
            .ToListAsync(ct);

        return new CashFlowReadResult
        {
            OpeningCashBalance = openingBalance,
            ClosingCashBalance = openingBalance + periodNet,
            Rows = rows
        };
    }

    public async Task<JournalReadResult> GetJournalAsync(JournalReadRequest request, CancellationToken ct = default)
    {
        var query = ApplyJournalFilters(_context.AccountingRegisterEntries.AsNoTracking(), request);
        var totalCount = await query.CountAsync(ct);
        var pageSize = request.PageSize;
        var skip = (request.Page - 1) * pageSize;

        var pageRows = await query
            .OrderBy(x => x.DocDate)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new JournalReadRow
            {
                Id = x.Id,
                PostingDate = x.DocDate,
                JournalNumber = x.JournalNumber,
                DocumentId = x.DocumentId,
                DocumentTypeId = x.DocumentTypeId,
                DocumentType = x.DocumentType.Name,
                Description = x.Content,
                DebitAccountCode = x.DebitAccount != null ? x.DebitAccount.Code : null,
                DebitAccountName = x.DebitAccount != null ? x.DebitAccount.Name : null,
                CreditAccountCode = x.CreditAccount != null ? x.CreditAccount.Code : null,
                CreditAccountName = x.CreditAccount != null ? x.CreditAccount.Name : null,
                Amount = x.Amount,
                CurrencyId = x.CurrencyId,
                Currency = x.Currency.Code,
                OrganizationId = x.OrganizationId,
                Organization = x.Organization.ShortName,
                Counterparty = x.RegisterEntrySubkontos
                    .Where(s => s.SubkontoTypeId == SubkontoTypeIdConst.Counterparties)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.DisplayValue)
                    .FirstOrDefault(),
                Warehouse = x.RegisterEntrySubkontos
                    .Where(s => s.SubkontoTypeId == SubkontoTypeIdConst.Warehouses)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.DisplayValue)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var documentNumbers = await GetDocumentNumbersAsync(pageRows, ct);
        foreach (var row in pageRows)
            row.DocumentNumber = documentNumbers.GetValueOrDefault((row.DocumentTypeId, row.DocumentId));

        return new JournalReadResult
        {
            TotalCount = totalCount,
            Rows = pageRows
        };
    }

    private async Task<decimal> GetCashBalanceAsync(
        DateTime? boundary,
        short? currencyId,
        bool beforeStart,
        CancellationToken ct,
        DateTime? startDate = null)
    {
        var query = _context.AccountingRegisterEntries.AsNoTracking();

        if (currencyId.HasValue)
            query = query.Where(x => x.CurrencyId == currencyId.Value);

        if (beforeStart)
        {
            if (boundary.HasValue)
                query = query.Where(x => x.DocDate < boundary.Value);
        }
        else
        {
            if (startDate.HasValue)
                query = query.Where(x => x.DocDate >= startDate.Value);

            if (boundary.HasValue)
                query = query.Where(x => x.DocDate <= boundary.Value);
        }

        var debit = await query
            .Where(x => x.DebitAccount != null && x.DebitAccount.Code!.StartsWith(CashPrefix))
            .Select(x => (decimal?)x.Amount)
            .SumAsync(ct) ?? 0m;

        var credit = await query
            .Where(x => x.CreditAccount != null && x.CreditAccount.Code!.StartsWith(CashPrefix))
            .Select(x => (decimal?)x.Amount)
            .SumAsync(ct) ?? 0m;

        return debit - credit;
    }

    private IQueryable<CashMovementRow> BuildCashInflowQuery(CashFlowReadRequest request) =>
        ApplyCashFilters(_context.AccountingRegisterEntries.AsNoTracking(), request)
            .Where(x => x.DebitAccount != null && x.DebitAccount.Code!.StartsWith(CashPrefix))
            .Select(x => new CashMovementRow
            {
                CounterpartAccountCode = x.CreditAccount != null ? x.CreditAccount.Code : null,
                CounterpartAccountName = x.CreditAccount != null ? x.CreditAccount.Name : null,
                Amount = x.Amount
            });

    private IQueryable<CashMovementRow> BuildCashOutflowQuery(CashFlowReadRequest request) =>
        ApplyCashFilters(_context.AccountingRegisterEntries.AsNoTracking(), request)
            .Where(x => x.CreditAccount != null && x.CreditAccount.Code!.StartsWith(CashPrefix))
            .Select(x => new CashMovementRow
            {
                CounterpartAccountCode = x.DebitAccount != null ? x.DebitAccount.Code : null,
                CounterpartAccountName = x.DebitAccount != null ? x.DebitAccount.Name : null,
                Amount = x.Amount
            });

    private static IQueryable<AccountingRegisterEntry> ApplyCashFilters(
        IQueryable<AccountingRegisterEntry> query,
        CashFlowReadRequest request)
    {
        if (request.DateFrom.HasValue)
            query = query.Where(x => x.DocDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(x => x.DocDate <= request.DateTo.Value);

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId.Value);

        return query;
    }

    private static IQueryable<AccountingRegisterEntry> ApplyJournalFilters(
        IQueryable<AccountingRegisterEntry> query,
        JournalReadRequest request)
    {
        if (request.DateFrom.HasValue)
            query = query.Where(x => x.DocDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(x => x.DocDate <= request.DateTo.Value);

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId.Value);

        if (request.DocumentTypeId.HasValue)
            query = query.Where(x => x.DocumentTypeId == request.DocumentTypeId.Value);

        return query;
    }

    private async Task<Dictionary<(short DocumentTypeId, long DocumentId), string>> GetDocumentNumbersAsync(
        List<JournalReadRow> rows,
        CancellationToken ct)
    {
        var result = new Dictionary<(short DocumentTypeId, long DocumentId), string>();

        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.PURCHASE, _context.PurchaseDocs.Select(x => new { x.Id, x.DocNumber }), result, ct);
        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.SALE, _context.SaleDocs.Select(x => new { x.Id, x.DocNumber }), result, ct);
        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.BANKOPERATION, _context.BankOperations.Select(x => new { x.Id, x.DocNumber }), result, ct);
        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.CASHOPERATION, _context.CashOperations.Select(x => new { x.Id, x.DocNumber }), result, ct);
        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.WAREHOUSETRANSFER, _context.WarehouseTransferDocs.Select(x => new { x.Id, x.DocNumber }), result, ct);
        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.INVENTORYADJUSTMENT, _context.InventoryAdjustmentDocs.Select(x => new { x.Id, x.DocNumber }), result, ct);
        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.INVENTORYCOUNT, _context.InventoryCountDocs.Select(x => new { x.Id, x.DocNumber }), result, ct);
        await AddDocumentNumbersAsync(rows, DocumentTypeIdConst.CASHCOLLECTION, _context.CashCollectionDocs.Select(x => new { x.Id, x.DocNumber }), result, ct);

        return result;
    }

    private static async Task AddDocumentNumbersAsync<TDocument>(
        List<JournalReadRow> rows,
        short documentTypeId,
        IQueryable<TDocument> source,
        Dictionary<(short DocumentTypeId, long DocumentId), string> target,
        CancellationToken ct)
        where TDocument : class
    {
        var ids = rows
            .Where(x => x.DocumentTypeId == documentTypeId)
            .Select(x => x.DocumentId)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return;

        var documents = await source
            .Where(BuildContainsExpression<TDocument>(ids))
            .Select(BuildDocumentProjection<TDocument>())
            .ToListAsync(ct);

        foreach (var document in documents)
            target[(documentTypeId, document.Id)] = document.DocNumber;
    }

    private static Expression<Func<TDocument, bool>> BuildContainsExpression<TDocument>(IReadOnlyCollection<long> ids)
        where TDocument : class
    {
        var parameter = Expression.Parameter(typeof(TDocument), "x");
        var idProperty = Expression.PropertyOrField(parameter, "Id");
        var containsMethod = typeof(Enumerable)
            .GetMethods()
            .Single(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(long));

        var body = Expression.Call(containsMethod, Expression.Constant(ids), idProperty);
        return Expression.Lambda<Func<TDocument, bool>>(body, parameter);
    }

    private static Expression<Func<TDocument, DocumentNumberRow>> BuildDocumentProjection<TDocument>()
        where TDocument : class
    {
        var parameter = Expression.Parameter(typeof(TDocument), "x");
        var idProperty = Expression.PropertyOrField(parameter, "Id");
        var docNumberProperty = Expression.PropertyOrField(parameter, "DocNumber");

        var body = Expression.MemberInit(
            Expression.New(typeof(DocumentNumberRow)),
            Expression.Bind(typeof(DocumentNumberRow).GetProperty(nameof(DocumentNumberRow.Id))!, idProperty),
            Expression.Bind(typeof(DocumentNumberRow).GetProperty(nameof(DocumentNumberRow.DocNumber))!, docNumberProperty));

        return Expression.Lambda<Func<TDocument, DocumentNumberRow>>(body, parameter);
    }

    private sealed class CashMovementRow
    {
        public string? CounterpartAccountCode { get; set; }
        public string? CounterpartAccountName { get; set; }
        public decimal Amount { get; set; }
    }

    private sealed class DocumentNumberRow
    {
        public long Id { get; set; }
        public string DocNumber { get; set; } = null!;
    }

    private const string CashPrefix = "5";
    private const string UnknownAccountCode = "UNASSIGNED";
    private const string UnknownAccountName = "Unassigned";
}
