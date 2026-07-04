using Application.Features.Acc.AccountingPeriods;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;

namespace Infrastructure.Repositories;

public class AccountingPeriodReadRepository : IAccountingPeriodReadRepository
{
    private readonly AppDbContext _context;

    public AccountingPeriodReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasOpenPreviousPeriodsAsync(int organizationId, DateOnly startDate, CancellationToken ct = default) =>
        _context.AccountingPeriods
            .AsNoTracking()
            .AnyAsync(x => x.OrganizationId == organizationId &&
                           x.StartDate < startDate &&
                           !x.IsClosed, ct);

    public Task<bool> HasLaterClosedPeriodsAsync(int organizationId, DateOnly startDate, CancellationToken ct = default) =>
        _context.AccountingPeriods
            .AsNoTracking()
            .AnyAsync(x => x.OrganizationId == organizationId &&
                           x.StartDate > startDate &&
                           x.IsClosed, ct);

    public async Task<int> CountUnconfirmedDocumentsAsync(int organizationId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default)
    {
        var purchaseCount = await _context.PurchaseDocs
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId &&
                             x.StateId == StateIdConst.ACTIVE &&
                             (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
                             x.DocDate >= dateFrom &&
                             x.DocDate <= dateTo, ct);

        var saleCount = await _context.SaleDocs
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId &&
                             x.StateId == StateIdConst.ACTIVE &&
                             (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
                             x.DocDate >= dateFrom &&
                             x.DocDate <= dateTo, ct);

        var bankCount = await _context.BankOperations
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId &&
                             x.StateId == StateIdConst.ACTIVE &&
                             (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
                             x.DocDate >= dateFrom &&
                             x.DocDate <= dateTo, ct);

        var cashCount = await _context.CashOperations
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId &&
                             x.StateId == StateIdConst.ACTIVE &&
                             (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
                             x.DocDate >= dateFrom &&
                             x.DocDate <= dateTo, ct);

        var transferCount = await _context.WarehouseTransferDocs
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId &&
                             x.StateId == StateIdConst.ACTIVE &&
                             (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
                             x.DocDate >= dateFrom &&
                             x.DocDate <= dateTo, ct);

        var adjustmentCount = await _context.InventoryAdjustmentDocs
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId &&
                             x.StateId == StateIdConst.ACTIVE &&
                             (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
                             x.DocDate >= dateFrom &&
                             x.DocDate <= dateTo, ct);

        var countCount = await _context.InventoryCountDocs
            .AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId &&
                             x.StateId == StateIdConst.ACTIVE &&
                             (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
                             x.DocDate >= dateFrom &&
                             x.DocDate <= dateTo, ct);

        return purchaseCount + saleCount + bankCount + cashCount + transferCount + adjustmentCount + countCount;
    }

    public async Task<bool> HasInvalidPostingBatchStateAsync(int organizationId, CancellationToken ct = default)
    {
        var hasUnknownStatus = await _context.PostingBatches
            .AsNoTracking()
            .AnyAsync(x => x.OrganizationId == organizationId &&
                           x.Status != PostingBatchStatusConst.POSTED &&
                           x.Status != PostingBatchStatusConst.REVERSED &&
                           x.Status != PostingBatchStatusConst.REVERSAL, ct);

        if (hasUnknownStatus)
            return true;

        return await _context.PostingBatches
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .GroupBy(x => new { x.DocumentTypeId, x.DocumentId })
            .AnyAsync(x => x.Count() > 1, ct);
    }
}
