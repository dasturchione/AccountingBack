using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FaCommissionings;

public interface IFaCommissioningService
{
    Task<Result<PagedResponse<FaCommissioningListDto>>> GetAllAsync(
        FaCommissioningListFilter filter,
        CancellationToken ct = default);

    Task<Result<FaCommissioningDto>> GetByIdAsync(
        long id,
        CancellationToken ct = default);

    Task<Result<long>> CreateAsync(
        FaCommissioningCreateDto dto,
        CancellationToken ct = default);

    Task<Result> UpdateAsync(
        long id,
        FaCommissioningUpdateDto dto,
        CancellationToken ct = default);

    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
