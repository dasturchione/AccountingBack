using Application.Common.Pagination;
using Application.Features.Acc.ChartAccounts;
using SharedKernel.Results;

namespace Application.Features.ChartAccounts;

public interface IChartAccountService
{
    Task<Result<PagedResponse<ChartAccountListDto>>> GetAllAsync(ChartAccountListFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<ChartAccountGroupedListDto>>> GetGroupedListAsync(ChartAccountListFilter filter, CancellationToken ct = default);
    Task<Result<ChartAccountDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(ChartAccountCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, ChartAccountUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
