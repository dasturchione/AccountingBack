using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.ChartAccountPresetAccounts;

public interface IChartAccountPresetAccountService
{
    Task<Result<PagedResponse<ChartAccountPresetAccountListDto>>> GetAllAsync(ChartAccountPresetAccountListFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<ChartAccountPresetAccountGroupedListDto>>> GetGroupedListAsync(ChartAccountPresetAccountListFilter filter, CancellationToken ct = default);
}
