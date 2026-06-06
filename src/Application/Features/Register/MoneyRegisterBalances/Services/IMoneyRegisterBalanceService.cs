using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public interface IMoneyRegisterBalanceService
{
    Task<Result<PagedResponse<MoneyRegisterBalanceListDto>>> GetAllAsync(MoneyRegisterBalanceListFilter filter, CancellationToken ct = default);
    Task<Result<MoneyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(MoneyRegisterBalanceCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, MoneyRegisterBalanceUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
