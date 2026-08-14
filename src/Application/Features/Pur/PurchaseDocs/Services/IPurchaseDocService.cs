using Application.Common.Pagination;
using Application.Abstractions.Integration.Edo;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public interface IPurchaseDocService
{
    Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default);
    Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<PurchaseDocPreviewDto>> PreviewAsync(PurchaseDocPreviewRequestDto request, CancellationToken ct = default);
    Task<Result<PurchaseDocDto>> CreateFromEdoAsync(PurchaseDocFromEdoRequestDto request, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}

public interface IEdoHistoricalPurchaseDraftFactory
{
    Task<Result<PurchaseDocDto>> CreateFromHistoricalSnapshotAsync(
        EdoDocumentDto document,
        PurchaseDocFromEdoRequestDto request,
        CancellationToken ct = default);
}
