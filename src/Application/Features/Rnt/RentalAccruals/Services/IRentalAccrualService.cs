using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Rnt.RentalAccruals;

public interface IRentalAccrualService
{
    Task<Result<PagedResponse<RentalAccrualDocListDto>>> GetAllAsync(RentalAccrualListFilter filter, CancellationToken ct = default);
    Task<Result<RentalAccrualDocDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, RentalAccrualUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> PostAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result<RentalAccrualGenerationResult>> GenerateDueAsync(DateTime? asOfDate, CancellationToken ct = default);
}

public interface IRentalAccrualLifecycleService
{
    Task<Result> PostAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}

public interface IRentalAccrualGenerationService
{
    Task<Result<RentalAccrualGenerationResult>> GenerateDueAsync(DateTime asOfDate, int? organizationId, CancellationToken ct = default);
}
