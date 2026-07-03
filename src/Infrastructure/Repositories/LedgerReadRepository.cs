using Application.Features.Ledger;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;

namespace Infrastructure.Repositories;

public class LedgerReadRepository : ILedgerReadRepository
{
    private readonly AppDbContext _context;

    public LedgerReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerReadResult> GetAsync(LedgerReadRequest request, CancellationToken ct = default)
    {
        var filteredMovements = BuildMovementsQuery(request, applyDateRange: true);
        var orderedMovements = filteredMovements
            .OrderBy(x => x.PostingDate)
            .ThenBy(x => x.Id);

        var totalCount = await filteredMovements.CountAsync(ct);
        var pageSize = request.PageSize;
        var skip = (request.Page - 1) * pageSize;

        var pageItems = await orderedMovements
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        var openingBalance = request.DateFrom.HasValue
            ? await SumNetAsync(BuildMovementsQuery(request, applyDateRange: false)
                .Where(x => x.PostingDate < request.DateFrom.Value), ct)
            : 0m;

        var filteredNet = await SumNetAsync(filteredMovements, ct);
        var pageOpeningBalance = openingBalance;

        if (pageItems.Count > 0)
        {
            var firstItem = pageItems[0];
            pageOpeningBalance += await SumNetAsync(
                filteredMovements.Where(x =>
                    x.PostingDate < firstItem.PostingDate ||
                    (x.PostingDate == firstItem.PostingDate && x.Id < firstItem.Id)),
                ct);
        }

        return new LedgerReadResult
        {
            OpeningBalance = openingBalance,
            ClosingBalance = openingBalance + filteredNet,
            PageOpeningBalance = pageOpeningBalance,
            TotalDebit = await SumAsync(filteredMovements.Select(x => (decimal?)x.Debit), ct),
            TotalCredit = await SumAsync(filteredMovements.Select(x => (decimal?)x.Credit), ct),
            TotalCount = totalCount,
            Transactions = await BuildTransactionsAsync(pageItems, ct)
        };
    }

    private async Task<List<LedgerReadTransaction>> BuildTransactionsAsync(
        List<LedgerMovementQueryRow> pageItems,
        CancellationToken ct)
    {
        if (pageItems.Count == 0)
            return [];

        var entryIds = pageItems.Select(x => x.Id).Distinct().ToList();

        var details = await _context.AccountingRegisterEntries
            .AsNoTracking()
            .Where(x => entryIds.Contains(x.Id))
            .Select(x => new LedgerEntryDetailRow
            {
                Id = x.Id,
                JournalNumber = x.JournalNumber,
                DocumentId = x.DocumentId,
                DocumentTypeId = x.DocumentTypeId,
                DocumentType = x.DocumentType.Name,
                Reference = x.RegisterEntrySubkontos
                    .Where(s => s.SubkontoTypeId == SubkontoTypeIdConst.PURCHASE ||
                                s.SubkontoTypeId == SubkontoTypeIdConst.SALE ||
                                s.SubkontoTypeId == SubkontoTypeIdConst.BANK_OPERATION)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.DisplayValue)
                    .FirstOrDefault(),
                Description = x.Content,
                CurrencyId = x.CurrencyId,
                Currency = x.Currency.Code,
                OrganizationId = x.OrganizationId,
                Organization = x.Organization.ShortName,
                CounterpartyId = x.RegisterEntrySubkontos
                    .Where(s => s.SubkontoTypeId == SubkontoTypeIdConst.COUNTER_PARTY)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.EntityId.HasValue ? (int?)s.EntityId.Value : null)
                    .FirstOrDefault(),
                Counterparty = x.RegisterEntrySubkontos
                    .Where(s => s.SubkontoTypeId == SubkontoTypeIdConst.COUNTER_PARTY)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.DisplayValue)
                    .FirstOrDefault(),
                WarehouseId = x.RegisterEntrySubkontos
                    .Where(s => s.SubkontoTypeId == SubkontoTypeIdConst.WAREHOUSE)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.EntityId.HasValue ? (int?)s.EntityId.Value : null)
                    .FirstOrDefault(),
                Warehouse = x.RegisterEntrySubkontos
                    .Where(s => s.SubkontoTypeId == SubkontoTypeIdConst.WAREHOUSE)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.DisplayValue)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var detailById = details.ToDictionary(x => x.Id);
        var documentNumbers = await GetDocumentNumbersAsync(details, ct);

        return pageItems
            .Select(row =>
            {
                var detail = detailById[row.Id];
                documentNumbers.TryGetValue((detail.DocumentTypeId, detail.DocumentId), out var documentNumber);

                return new LedgerReadTransaction
                {
                    Id = row.Id,
                    PostingDate = row.PostingDate,
                    JournalNumber = detail.JournalNumber,
                    DocumentNumber = documentNumber,
                    DocumentTypeId = detail.DocumentTypeId,
                    DocumentType = detail.DocumentType,
                    Reference = detail.Reference,
                    Description = detail.Description,
                    Debit = row.Debit,
                    Credit = row.Credit,
                    CurrencyId = detail.CurrencyId,
                    Currency = detail.Currency,
                    OrganizationId = detail.OrganizationId,
                    Organization = detail.Organization,
                    CounterpartyId = detail.CounterpartyId,
                    Counterparty = detail.Counterparty,
                    WarehouseId = detail.WarehouseId,
                    Warehouse = detail.Warehouse
                };
            })
            .ToList();
    }

    private async Task<Dictionary<(short DocumentTypeId, long DocumentId), string>> GetDocumentNumbersAsync(
        List<LedgerEntryDetailRow> details,
        CancellationToken ct)
    {
        var result = new Dictionary<(short DocumentTypeId, long DocumentId), string>();

        await AddDocumentNumbersAsync(
            details,
            DocumentTypeIdConst.PURCHASE,
            _context.PurchaseDocs.Select(x => new { x.Id, x.DocNumber }),
            result,
            ct);

        await AddDocumentNumbersAsync(
            details,
            DocumentTypeIdConst.SALE,
            _context.SaleDocs.Select(x => new { x.Id, x.DocNumber }),
            result,
            ct);

        await AddDocumentNumbersAsync(
            details,
            DocumentTypeIdConst.BANKOPERATION,
            _context.BankOperations.Select(x => new { x.Id, x.DocNumber }),
            result,
            ct);

        await AddDocumentNumbersAsync(
            details,
            DocumentTypeIdConst.CASHOPERATION,
            _context.CashOperations.Select(x => new { x.Id, x.DocNumber }),
            result,
            ct);

        await AddDocumentNumbersAsync(
            details,
            DocumentTypeIdConst.WAREHOUSETRANSFER,
            _context.WarehouseTransferDocs.Select(x => new { x.Id, x.DocNumber }),
            result,
            ct);

        await AddDocumentNumbersAsync(
            details,
            DocumentTypeIdConst.INVENTORYADJUSTMENT,
            _context.InventoryAdjustmentDocs.Select(x => new { x.Id, x.DocNumber }),
            result,
            ct);

        await AddDocumentNumbersAsync(
            details,
            DocumentTypeIdConst.INVENTORYCOUNT,
            _context.InventoryCountDocs.Select(x => new { x.Id, x.DocNumber }),
            result,
            ct);

        return result;
    }

    private static async Task AddDocumentNumbersAsync<TDocument>(
        List<LedgerEntryDetailRow> details,
        short documentTypeId,
        IQueryable<TDocument> source,
        Dictionary<(short DocumentTypeId, long DocumentId), string> target,
        CancellationToken ct)
        where TDocument : class
    {
        var ids = details
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

    private IQueryable<LedgerMovementQueryRow> BuildMovementsQuery(LedgerReadRequest request, bool applyDateRange)
    {
        var debitEntries = ApplyEntryFilters(
                _context.AccountingRegisterEntries
                    .AsNoTracking()
                    .Where(x => x.DebitAccountId == request.AccountId),
                request,
                applyDateRange)
            .Select(x => new LedgerMovementQueryRow
            {
                Id = x.Id,
                PostingDate = x.DocDate,
                Debit = x.Amount,
                Credit = 0m
            });

        var creditEntries = ApplyEntryFilters(
                _context.AccountingRegisterEntries
                    .AsNoTracking()
                    .Where(x => x.CreditAccountId == request.AccountId),
                request,
                applyDateRange)
            .Select(x => new LedgerMovementQueryRow
            {
                Id = x.Id,
                PostingDate = x.DocDate,
                Debit = 0m,
                Credit = x.Amount
            });

        return debitEntries.Concat(creditEntries);
    }

    private IQueryable<AccountingRegisterEntry> ApplyEntryFilters(
        IQueryable<AccountingRegisterEntry> query,
        LedgerReadRequest request,
        bool applyDateRange)
    {
        if (applyDateRange && request.DateFrom.HasValue)
            query = query.Where(x => x.DocDate >= request.DateFrom.Value);

        if (applyDateRange && request.DateTo.HasValue)
            query = query.Where(x => x.DocDate <= request.DateTo.Value);

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId.Value);

        if (request.CounterpartyId.HasValue)
        {
            query = query.Where(x => x.RegisterEntrySubkontos.Any(s =>
                s.SubkontoTypeId == SubkontoTypeIdConst.COUNTER_PARTY &&
                s.EntityId == request.CounterpartyId.Value));
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.RegisterEntrySubkontos.Any(s =>
                s.SubkontoTypeId == SubkontoTypeIdConst.WAREHOUSE &&
                s.EntityId == request.WarehouseId.Value));
        }

        return query;
    }

    private static async Task<decimal> SumNetAsync(IQueryable<LedgerMovementQueryRow> query, CancellationToken ct) =>
        await SumAsync(query.Select(x => (decimal?)(x.Debit - x.Credit)), ct);

    private static async Task<decimal> SumAsync(IQueryable<decimal?> query, CancellationToken ct) =>
        (await query.SumAsync(ct)) ?? 0m;

    private static System.Linq.Expressions.Expression<Func<TDocument, bool>> BuildContainsExpression<TDocument>(IReadOnlyCollection<long> ids)
        where TDocument : class
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TDocument), "x");
        var idProperty = System.Linq.Expressions.Expression.PropertyOrField(parameter, "Id");
        var containsMethod = typeof(Enumerable)
            .GetMethods()
            .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(long));

        var body = System.Linq.Expressions.Expression.Call(
            containsMethod,
            System.Linq.Expressions.Expression.Constant(ids),
            idProperty);

        return System.Linq.Expressions.Expression.Lambda<Func<TDocument, bool>>(body, parameter);
    }

    private static System.Linq.Expressions.Expression<Func<TDocument, LedgerDocumentNumberRow>> BuildDocumentProjection<TDocument>()
        where TDocument : class
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TDocument), "x");
        var idProperty = System.Linq.Expressions.Expression.PropertyOrField(parameter, "Id");
        var docNumberProperty = System.Linq.Expressions.Expression.PropertyOrField(parameter, "DocNumber");

        var constructor = typeof(LedgerDocumentNumberRow).GetConstructor(Type.EmptyTypes)!;
        var bindings = new[]
        {
            System.Linq.Expressions.Expression.Bind(typeof(LedgerDocumentNumberRow).GetProperty(nameof(LedgerDocumentNumberRow.Id))!, idProperty),
            System.Linq.Expressions.Expression.Bind(typeof(LedgerDocumentNumberRow).GetProperty(nameof(LedgerDocumentNumberRow.DocNumber))!, docNumberProperty)
        };

        var body = System.Linq.Expressions.Expression.MemberInit(
            System.Linq.Expressions.Expression.New(constructor),
            bindings);

        return System.Linq.Expressions.Expression.Lambda<Func<TDocument, LedgerDocumentNumberRow>>(body, parameter);
    }

    private sealed class LedgerMovementQueryRow
    {
        public long Id { get; set; }
        public DateTime PostingDate { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    private sealed class LedgerEntryDetailRow
    {
        public long Id { get; set; }
        public string? JournalNumber { get; set; }
        public long DocumentId { get; set; }
        public short DocumentTypeId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string? Reference { get; set; }
        public string? Description { get; set; }
        public short CurrencyId { get; set; }
        public string Currency { get; set; } = null!;
        public int OrganizationId { get; set; }
        public string Organization { get; set; } = null!;
        public int? CounterpartyId { get; set; }
        public string? Counterparty { get; set; }
        public int? WarehouseId { get; set; }
        public string? Warehouse { get; set; }
    }

    private sealed class LedgerDocumentNumberRow
    {
        public long Id { get; set; }
        public string DocNumber { get; set; } = null!;
    }
}
