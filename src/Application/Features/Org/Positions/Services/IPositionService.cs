using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Positions;

public interface IPositionService
{
    Task<Result<PagedResponse<PositionListDto>>> GetAllAsync(PositionListFilter filter, CancellationToken ct = default);
    Task<Result<PositionDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(PositionCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, PositionUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
