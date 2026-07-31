using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Inv.OpeningInventories;

public interface IOpeningInventoryService
{
    Task<Result<PagedResponse<OpeningInventoryListDto>>> GetAllAsync(
        OpeningInventoryListFilter filter,
        CancellationToken ct = default);

    Task<Result<OpeningInventoryDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(OpeningInventoryCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, OpeningInventoryUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
