using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.RetailSaleDocs;

public interface IRetailSaleDocService
{
    Task<Result<PagedResponse<RetailSaleDocListDto>>> GetAllAsync(RetailSaleDocListFilter filter, CancellationToken ct = default);
    Task<Result<RetailSaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(RetailSaleDocCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, RetailSaleDocUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, RetailSaleDocConfirmDto dto, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
