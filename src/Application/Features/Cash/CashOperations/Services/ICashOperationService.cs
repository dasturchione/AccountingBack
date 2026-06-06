using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CashOperations;

public interface ICashOperationService
{
    Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default);
    Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(CashOperationCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, CashOperationUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
