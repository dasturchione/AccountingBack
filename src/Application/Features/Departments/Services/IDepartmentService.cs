using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Departments;

public interface IDepartmentService
{
    Task<Result<PagedResponse<DepartmentListDto>>> GetAllAsync(DepartmentListFilter filter, CancellationToken ct = default);
    Task<Result<DepartmentDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(DepartmentCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, DepartmentUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
