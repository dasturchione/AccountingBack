using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.SaleDocTables;

public interface ISaleDocTableService
{
    Task<Result<PagedResponse<SaleDocTableListDto>>> GetAllAsync(SaleDocTableListFilter filter, CancellationToken ct = default);
    Task<Result<SaleDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(SaleDocTableCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, SaleDocTableUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
