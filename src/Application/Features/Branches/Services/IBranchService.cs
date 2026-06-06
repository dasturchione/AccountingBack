using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Branches;

public interface IBranchService
{
    Task<Result<PagedResponse<BranchListDto>>> GetAllAsync(BranchListFilter filter, CancellationToken ct = default);
    Task<Result<BranchDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(BranchCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, BranchUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
