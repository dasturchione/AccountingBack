using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public interface IPurchaseDocService
{
    Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default);
    Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
