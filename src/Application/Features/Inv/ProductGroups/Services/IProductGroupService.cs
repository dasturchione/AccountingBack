using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.ProductGroups;

public interface IProductGroupService
{
    Task<Result<PagedResponse<ProductGroupListDto>>> GetAllAsync(ProductGroupListFilter filter, CancellationToken ct = default);
    Task<Result<ProductGroupDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(ProductGroupCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, ProductGroupUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
