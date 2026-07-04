using Application.Common.Pagination;
using Application.Features.PurchaseDocs;
using SharedKernel.Results;

namespace Application.Features.Reports.PurchaseReports;

public interface IPurchaseReportService
{
    Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default);
    Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default);
}

public sealed class PurchaseReportService : IPurchaseReportService
{
    private readonly IPurchaseDocService _purchaseDocService;

    public PurchaseReportService(IPurchaseDocService purchaseDocService)
    {
        _purchaseDocService = purchaseDocService;
    }

    public Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default) =>
        _purchaseDocService.GetAllAsync(filter, ct);

    public Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        _purchaseDocService.GetByIdAsync(id, ct);
}
