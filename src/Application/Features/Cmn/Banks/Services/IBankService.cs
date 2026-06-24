using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Banks;

public interface IBankService
{
    Task<Result<PagedResponse<BankListDto>>> GetAllAsync(BankListFilter filter, CancellationToken ct = default);
    Task<Result<BankDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(BankCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, BankUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
