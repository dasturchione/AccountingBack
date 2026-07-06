using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FaReceipts;

public interface IFaReceiptService
{
    Task<Result<PagedResponse<FaReceiptListDto>>> GetAllAsync(FaReceiptListFilter filter, CancellationToken ct = default);
    Task<Result<FaReceiptDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(FaReceiptCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, FaReceiptUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
