using SharedKernel.Results;

namespace Application.Features.Platform;

public interface IDashboardService
{
    Task<Result<DashboardStatsDto>> GetStatsAsync(CancellationToken ct = default);
}
