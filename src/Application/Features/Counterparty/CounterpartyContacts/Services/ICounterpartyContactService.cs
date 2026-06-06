using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CounterpartyContacts;

public interface ICounterpartyContactService
{
    Task<Result<PagedResponse<CounterpartyContactListDto>>> GetAllAsync(CounterpartyContactListFilter filter, CancellationToken ct = default);
    Task<Result<CounterpartyContactDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(CounterpartyContactCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, CounterpartyContactUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
