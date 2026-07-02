using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public interface IBankOperationService
{
    Task<Result<PagedResponse<BankOperationListDto>>> GetAllAsync(BankOperationListFilter filter, CancellationToken ct = default);
    Task<Result<BankOperationDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(BankOperationCreateDto dto, CancellationToken ct = default);
    Task<Result<List<long>>> CreateManyAsync(BankOperationsCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, BankOperationUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
