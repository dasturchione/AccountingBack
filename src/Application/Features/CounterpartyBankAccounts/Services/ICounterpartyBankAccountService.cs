using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CounterpartyBankAccounts;

public interface ICounterpartyBankAccountService
{
    Task<Result<PagedResponse<CounterpartyBankAccountListDto>>> GetAllAsync(CounterpartyBankAccountListFilter filter, CancellationToken ct = default);
    Task<Result<CounterpartyBankAccountDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(CounterpartyBankAccountCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, CounterpartyBankAccountUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
