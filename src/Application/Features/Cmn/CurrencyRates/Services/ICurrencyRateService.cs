using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRates;

public interface ICurrencyRateService
{
    Task<Result<PagedResponse<CurrencyRateListDto>>> GetAllAsync(CurrencyRateListFilter filter, CancellationToken ct = default);
    Task<Result<CurrencyRateDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<CurrencyRateDto>> GetLatestAsync(short baseCurrencyId, short targetCurrencyId, CancellationToken ct = default);
    Task<Result<PagedResponse<CurrencyRateListDto>>> GetHistoryAsync(short baseCurrencyId, short targetCurrencyId, CurrencyRateListFilter filter, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(CurrencyRateCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, CurrencyRateUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
