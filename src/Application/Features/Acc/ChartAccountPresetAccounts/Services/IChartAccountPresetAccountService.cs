using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.ChartAccountPresetAccounts;

public interface IChartAccountPresetAccountService
{
    Task<Result<PagedResponse<ChartAccountPresetAccountListDto>>> GetGroupedListAsync(ChartAccountPresetAccountListFilter filter, CancellationToken ct = default);
}
