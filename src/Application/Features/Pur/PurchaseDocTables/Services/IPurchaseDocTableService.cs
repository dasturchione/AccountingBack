using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public interface IPurchaseDocTableService
{
    Task<Result<PagedResponse<PurchaseDocTableListDto>>> GetAllAsync(PurchaseDocTableListFilter filter, CancellationToken ct = default);
    Task<Result<PurchaseDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PurchaseDocTableCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PurchaseDocTableUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
