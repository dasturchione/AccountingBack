using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CashBoxes;

public interface ICashBoxService
{
    Task<Result<PagedResponse<CashBoxListDto>>> GetAllAsync(CashBoxListFilter filter, CancellationToken ct = default);
    Task<Result<CashBoxDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(CashBoxCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, CashBoxUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
