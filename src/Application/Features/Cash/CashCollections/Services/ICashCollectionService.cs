using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CashCollections;

public interface ICashCollectionService
{
    Task<Result<PagedResponse<CashCollectionListDto>>> GetAllAsync(CashCollectionListFilter filter, CancellationToken ct = default);
    Task<Result<CashCollectionDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<List<CashCollectionInTransitDto>>> GetInTransitAsync(int? bankAccountId, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(CashCollectionCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, CashCollectionUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> SendToBankAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}

public interface ICashCollectionLifecycleService
{
    Task<Result> SendToBankAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
