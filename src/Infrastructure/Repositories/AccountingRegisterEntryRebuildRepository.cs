using Application.Features.AccountingRegisterEntries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;

namespace Infrastructure.Repositories;

public sealed class AccountingRegisterEntryRebuildRepository : IAccountingRegisterEntryRebuildRepository
{
    private static readonly HashSet<short> SupportedDocumentTypes =
    [
        DocumentTypeIdConst.PURCHASE,
        DocumentTypeIdConst.SALE,
        DocumentTypeIdConst.BANKOPERATION,
        DocumentTypeIdConst.CASHOPERATION,
        DocumentTypeIdConst.SALARY,
        DocumentTypeIdConst.RETAIL_SALE,
        DocumentTypeIdConst.CURRENCYREVALUATION,
        DocumentTypeIdConst.FARECEIPT,
        DocumentTypeIdConst.FADEPRECIATION,
        DocumentTypeIdConst.FADISPOSAL,
        DocumentTypeIdConst.FAREVALUATION,
        DocumentTypeIdConst.FACOMMISSIONING,
        DocumentTypeIdConst.CASHFISCALTRANSFER,
        DocumentTypeIdConst.CASHCOLLECTION
    ];

    private readonly AppDbContext _context;

    public AccountingRegisterEntryRebuildRepository(AppDbContext context)
    {
        _context = context;
    }

    public bool Supports(short documentTypeId) => SupportedDocumentTypes.Contains(documentTypeId);

    public async Task<AccountingRegisterEntryRebuildSource?> GetSourceAsync(
        short documentTypeId,
        long documentId,
        int organizationId,
        CancellationToken ct = default)
    {
        var document = await GetDocumentAsync(documentTypeId, documentId, organizationId, ct);
        if (document is null)
            return null;

        var postingBatchId = await _context.PostingBatches
            .AsNoTracking()
            .Where(batch =>
                batch.OrganizationId == organizationId &&
                batch.DocumentTypeId == documentTypeId &&
                batch.DocumentId == documentId &&
                batch.Status == PostingBatchStatusConst.POSTED)
            .OrderByDescending(batch => batch.PostedAt)
            .ThenByDescending(batch => batch.Id)
            .Select(batch => (long?)batch.Id)
            .FirstOrDefaultAsync(ct);

        return new AccountingRegisterEntryRebuildSource(document, postingBatchId);
    }

    public Task<int> DeleteEntriesAsync(
        short documentTypeId,
        long documentId,
        int organizationId,
        CancellationToken ct = default) =>
        _context.AccountingRegisterEntries
            .Where(entry =>
                entry.OrganizationId == organizationId &&
                entry.DocumentTypeId == documentTypeId &&
                entry.DocumentId == documentId)
            .ExecuteDeleteAsync(ct);

    private Task<object?> GetDocumentAsync(
        short documentTypeId,
        long documentId,
        int organizationId,
        CancellationToken ct) =>
        documentTypeId switch
        {
            DocumentTypeIdConst.PURCHASE => GetPurchaseAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.SALE => GetSaleAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.BANKOPERATION => GetBankOperationAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.CASHOPERATION => GetCashOperationAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.SALARY => GetPayrollAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.RETAIL_SALE => GetRetailSaleAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.CURRENCYREVALUATION => GetCurrencyRevaluationAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.FARECEIPT => GetFaReceiptAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.FADEPRECIATION => GetFaDepreciationAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.FADISPOSAL => GetFaDisposalAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.FAREVALUATION => GetFaRevaluationAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.FACOMMISSIONING => GetFaCommissioningAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.CASHFISCALTRANSFER => GetCashFiscalTransferAsync(documentId, organizationId, ct),
            DocumentTypeIdConst.CASHCOLLECTION => GetCashCollectionAsync(documentId, organizationId, ct),
            _ => Task.FromResult<object?>(null)
        };

    private async Task<object?> GetPurchaseAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.PurchaseDocs
            .AsNoTracking()
            .Include(document => document.PurchaseDocProducts)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetSaleAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.SaleDocs
            .AsNoTracking()
            .Include(document => document.SaleDocProducts)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetBankOperationAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.BankOperations
            .AsNoTracking()
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetCashOperationAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.CashOperations
            .AsNoTracking()
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetPayrollAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.PayPayrollDocs
            .AsNoTracking()
            .AsSplitQuery()
            .Include(document => document.Lines)
                .ThenInclude(line => line.Employee)
            .Include(document => document.Lines)
                .ThenInclude(line => line.Employment)
                    .ThenInclude(employment => employment.Department)
            .Include(document => document.Lines)
                .ThenInclude(line => line.CalcLines)
                    .ThenInclude(line => line.Component)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetRetailSaleAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.RetailSaleDocs
            .AsNoTracking()
            .AsSplitQuery()
            .Include(document => document.RetailSaleDocProducts)
                .ThenInclude(line => line.Product)
            .Include(document => document.RetailSaleDocPayments)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetCurrencyRevaluationAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.CurrencyRevaluations
            .AsNoTracking()
            .Include(document => document.Lines)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetFaReceiptAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.FaReceiptDocs
            .AsNoTracking()
            .AsSplitQuery()
            .Include(document => document.Lines)
                .ThenInclude(line => line.Assets)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetFaDepreciationAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.FaDepreciationRuns
            .AsNoTracking()
            .Include(document => document.Lines)
                .ThenInclude(line => line.FaAsset)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetFaDisposalAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.FaDisposalDocs
            .AsNoTracking()
            .Include(document => document.Lines)
                .ThenInclude(line => line.FaAsset)
                    .ThenInclude(asset => asset.FaAssetAccounting)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetFaRevaluationAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.FaRevaluationDocs
            .AsNoTracking()
            .Include(document => document.Lines)
                .ThenInclude(line => line.FaAsset)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetFaCommissioningAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.FaCommissioningDocs
            .AsNoTracking()
            .Include(document => document.Lines)
                .ThenInclude(line => line.FaAsset)
                    .ThenInclude(asset => asset.FaAssetAccounting)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetCashFiscalTransferAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.CashFiscalTransferDocs
            .AsNoTracking()
            .Include(document => document.CashBox)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                document.StatusId == DocumentStatusIdConst.POSTED,
                ct);

    private async Task<object?> GetCashCollectionAsync(long id, int organizationId, CancellationToken ct) =>
        await _context.CashCollectionDocs
            .AsNoTracking()
            .Include(document => document.CashBox)
            .Include(document => document.BankAccount)
            .SingleOrDefaultAsync(document =>
                document.Id == id &&
                document.OrganizationId == organizationId &&
                (document.StatusId == DocumentStatusIdConst.IN_TRANSIT ||
                 document.StatusId == DocumentStatusIdConst.COMPLETED),
                ct);
}
