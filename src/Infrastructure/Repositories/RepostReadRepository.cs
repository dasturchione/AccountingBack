using Application.Features.Reposting;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class RepostReadRepository : IRepostReadRepository
{
    private readonly AppDbContext _context;

    public RepostReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RepostCandidate>> GetCandidatesAsync(RepostReadRequest request, CancellationToken ct = default)
    {
        var result = new List<RepostCandidate>();

        if (ShouldInclude(request.DocumentType, DocumentTypeIdConst.PURCHASE))
            result.AddRange(await QueryPurchaseDocsAsync(request, ct));

        if (ShouldInclude(request.DocumentType, DocumentTypeIdConst.SALE))
            result.AddRange(await QuerySaleDocsAsync(request, ct));

        if (ShouldInclude(request.DocumentType, DocumentTypeIdConst.BANKOPERATION))
            result.AddRange(await QueryBankOperationsAsync(request, ct));

        if (ShouldInclude(request.DocumentType, DocumentTypeIdConst.CASHOPERATION))
            result.AddRange(await QueryCashOperationsAsync(request, ct));

        if (ShouldInclude(request.DocumentType, DocumentTypeIdConst.WAREHOUSETRANSFER))
            result.AddRange(await QueryWarehouseTransfersAsync(request, ct));

        if (ShouldInclude(request.DocumentType, DocumentTypeIdConst.INVENTORYADJUSTMENT))
            result.AddRange(await QueryInventoryAdjustmentsAsync(request, ct));

        if (ShouldInclude(request.DocumentType, DocumentTypeIdConst.INVENTORYCOUNT))
            result.AddRange(await QueryInventoryCountsAsync(request, ct));

        return result
            .OrderBy(x => x.DocDate)
            .ThenBy(x => x.DocumentType)
            .ThenBy(x => x.DocumentId)
            .ToList();
    }

    private Task<List<RepostCandidate>> QueryPurchaseDocsAsync(RepostReadRequest request, CancellationToken ct) =>
        ApplyCommonFilters(_context.PurchaseDocs.AsNoTracking(), request, x => x.OrganizationId, x => x.Id, x => x.DocDate)
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new RepostCandidate
            {
                DocumentType = DocumentTypeIdConst.PURCHASE,
                DocumentId = x.Id,
                DocDate = x.DocDate
            })
            .ToListAsync(ct);

    private Task<List<RepostCandidate>> QuerySaleDocsAsync(RepostReadRequest request, CancellationToken ct) =>
        ApplyCommonFilters(_context.SaleDocs.AsNoTracking(), request, x => x.OrganizationId, x => x.Id, x => x.DocDate)
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new RepostCandidate
            {
                DocumentType = DocumentTypeIdConst.SALE,
                DocumentId = x.Id,
                DocDate = x.DocDate
            })
            .ToListAsync(ct);

    private Task<List<RepostCandidate>> QueryBankOperationsAsync(RepostReadRequest request, CancellationToken ct) =>
        ApplyCommonFilters(_context.BankOperations.AsNoTracking(), request, x => x.OrganizationId, x => x.Id, x => x.DocDate)
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new RepostCandidate
            {
                DocumentType = DocumentTypeIdConst.BANKOPERATION,
                DocumentId = x.Id,
                DocDate = x.DocDate
            })
            .ToListAsync(ct);

    private Task<List<RepostCandidate>> QueryCashOperationsAsync(RepostReadRequest request, CancellationToken ct) =>
        ApplyCommonFilters(_context.CashOperations.AsNoTracking(), request, x => x.OrganizationId, x => x.Id, x => x.DocDate)
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new RepostCandidate
            {
                DocumentType = DocumentTypeIdConst.CASHOPERATION,
                DocumentId = x.Id,
                DocDate = x.DocDate
            })
            .ToListAsync(ct);

    private Task<List<RepostCandidate>> QueryWarehouseTransfersAsync(RepostReadRequest request, CancellationToken ct) =>
        ApplyCommonFilters(_context.WarehouseTransferDocs.AsNoTracking(), request, x => x.OrganizationId, x => x.Id, x => x.DocDate)
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new RepostCandidate
            {
                DocumentType = DocumentTypeIdConst.WAREHOUSETRANSFER,
                DocumentId = x.Id,
                DocDate = x.DocDate
            })
            .ToListAsync(ct);

    private Task<List<RepostCandidate>> QueryInventoryAdjustmentsAsync(RepostReadRequest request, CancellationToken ct) =>
        ApplyCommonFilters(_context.InventoryAdjustmentDocs.AsNoTracking(), request, x => x.OrganizationId, x => x.Id, x => x.DocDate)
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new RepostCandidate
            {
                DocumentType = DocumentTypeIdConst.INVENTORYADJUSTMENT,
                DocumentId = x.Id,
                DocDate = x.DocDate
            })
            .ToListAsync(ct);

    private Task<List<RepostCandidate>> QueryInventoryCountsAsync(RepostReadRequest request, CancellationToken ct) =>
        ApplyCommonFilters(_context.InventoryCountDocs.AsNoTracking(), request, x => x.OrganizationId, x => x.Id, x => x.DocDate)
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new RepostCandidate
            {
                DocumentType = DocumentTypeIdConst.INVENTORYCOUNT,
                DocumentId = x.Id,
                DocDate = x.DocDate
            })
            .ToListAsync(ct);

    private static IQueryable<TDocument> ApplyCommonFilters<TDocument>(
        IQueryable<TDocument> query,
        RepostReadRequest request,
        Expression<Func<TDocument, int>> organizationIdSelector,
        Expression<Func<TDocument, long>> idSelector,
        Expression<Func<TDocument, DateTime>> docDateSelector)
        where TDocument : class
    {
        query = query.Where(BuildEquals(organizationIdSelector, request.OrganizationId));

        if (request.DocumentId.HasValue)
            query = query.Where(BuildEquals(idSelector, request.DocumentId.Value));

        if (request.DateFrom.HasValue)
            query = query.Where(BuildGreaterThanOrEqual(docDateSelector, request.DateFrom.Value));

        if (request.DateTo.HasValue)
            query = query.Where(BuildLessThanOrEqual(docDateSelector, request.DateTo.Value));

        return query;
    }

    private static Expression<Func<TDocument, bool>> BuildEquals<TDocument, TValue>(
        Expression<Func<TDocument, TValue>> selector,
        TValue value)
    {
        var parameter = selector.Parameters[0];
        var body = Expression.Equal(selector.Body, Expression.Constant(value, typeof(TValue)));
        return Expression.Lambda<Func<TDocument, bool>>(body, parameter);
    }

    private static Expression<Func<TDocument, bool>> BuildGreaterThanOrEqual<TDocument>(
        Expression<Func<TDocument, DateTime>> selector,
        DateTime value)
    {
        var parameter = selector.Parameters[0];
        var body = Expression.GreaterThanOrEqual(selector.Body, Expression.Constant(value));
        return Expression.Lambda<Func<TDocument, bool>>(body, parameter);
    }

    private static Expression<Func<TDocument, bool>> BuildLessThanOrEqual<TDocument>(
        Expression<Func<TDocument, DateTime>> selector,
        DateTime value)
    {
        var parameter = selector.Parameters[0];
        var body = Expression.LessThanOrEqual(selector.Body, Expression.Constant(value));
        return Expression.Lambda<Func<TDocument, bool>>(body, parameter);
    }

    private static bool ShouldInclude(short? filterDocumentType, short documentType) =>
        !filterDocumentType.HasValue || filterDocumentType.Value == documentType;
}
