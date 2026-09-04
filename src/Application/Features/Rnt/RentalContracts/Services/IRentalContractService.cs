using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Rnt.RentalContracts;

public interface IRentalContractService
{
    Task<Result<PagedResponse<RentalContractListDto>>> GetAllAsync(RentalContractListFilter filter, CancellationToken ct = default);
    Task<Result<RentalContractDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(RentalContractCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, RentalContractUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> ActivateAsync(long id, DateTime? confirmationDate = null, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, DateTime? terminationDate = null, CancellationToken ct = default);
}
