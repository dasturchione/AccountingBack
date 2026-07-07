using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FaMovements;

public interface IFaMovementService
{
    Task<Result<PagedResponse<FaMovementListDto>>> GetAllAsync(FaMovementListFilter filter, CancellationToken ct = default);
    Task<Result<FaMovementDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(FaMovementCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, FaMovementUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
