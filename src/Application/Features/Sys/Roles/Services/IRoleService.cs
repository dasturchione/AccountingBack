using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Roles;

public interface IRoleService
{
    Task<Result<PagedResponse<RoleListDto>>> GetAllAsync(RoleListFilter filter, CancellationToken ct = default);
    Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(RoleCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, RoleUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
