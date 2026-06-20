using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public interface ISaleDocService
{
    Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default);
    Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(SaleDocCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, SaleDocUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, SaleDocConfirmDto dto, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
