using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public interface ICounterpartyRegisterBalanceService
{
    Task<Result<PagedResponse<CounterpartyRegisterBalanceListDto>>> GetAllAsync(CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default);
    Task<Result<CounterpartyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(CounterpartyRegisterBalanceCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, CounterpartyRegisterBalanceUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
