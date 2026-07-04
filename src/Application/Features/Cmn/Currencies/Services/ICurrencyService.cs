using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Cmn.Currencies;

public interface ICurrencyService
{
    Task<Result<PagedResponse<CurrencyListDto>>> GetAllAsync(CurrencyListFilter filter, CancellationToken ct = default);
    Task<Result<CurrencyDto>> GetByIdAsync(short id, CancellationToken ct = default);
    Task<Result<short>> CreateAsync(CurrencyCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(short id, CurrencyUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(short id, CancellationToken ct = default);
}
