using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public interface ITaxService
{
    Task<Result<PagedResponse<TaxListDto>>> GetPagedAsync(TaxListFilter filter, CancellationToken ct = default);
    Task<Result<TaxDto>> GetByIdAsync(short id, CancellationToken ct = default);
    Task<Result<short>> CreateAsync(TaxCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(short id, TaxUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(short id, CancellationToken ct = default);
}
