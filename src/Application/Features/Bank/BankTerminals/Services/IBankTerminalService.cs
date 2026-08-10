using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.BankTerminals;

public interface IBankTerminalService
{
    Task<Result<PagedResponse<BankTerminalListDto>>> GetAllAsync(BankTerminalListFilter filter, CancellationToken ct = default);
    Task<Result<BankTerminalDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(BankTerminalCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, BankTerminalUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
