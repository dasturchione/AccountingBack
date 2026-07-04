using SharedKernel.Results;

namespace Application.Features.TrialBalance;

public interface ITrialBalanceService
{
    Task<Result<TrialBalanceDto>> GetAsync(TrialBalanceFilter filter, CancellationToken ct = default);
}
