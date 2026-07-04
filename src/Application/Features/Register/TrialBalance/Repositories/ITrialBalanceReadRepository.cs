namespace Application.Features.TrialBalance;

public interface ITrialBalanceReadRepository
{
    Task<TrialBalanceReadResult> GetAsync(TrialBalanceReadRequest request, CancellationToken ct = default);
}
