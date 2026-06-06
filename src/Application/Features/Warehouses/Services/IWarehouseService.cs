using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Warehouses;

public interface IWarehouseService
{
    Task<Result<PagedResponse<WarehouseListDto>>> GetAllAsync(WarehouseListFilter filter, CancellationToken ct = default);
    Task<Result<WarehouseDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(WarehouseCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, WarehouseUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
