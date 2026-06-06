using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CounterpartyCards;

public interface ICounterpartyCardService
{
    Task<Result<PagedResponse<CounterpartyCardListDto>>> GetAllAsync(CounterpartyCardListFilter filter, CancellationToken ct = default);
    Task<Result<CounterpartyCardDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(CounterpartyCardCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, CounterpartyCardUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
