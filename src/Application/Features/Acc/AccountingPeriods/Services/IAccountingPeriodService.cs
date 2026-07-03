using SharedKernel.Results;

namespace Application.Features.Acc.AccountingPeriods;

public interface IAccountingPeriodService
{
    Task<Result> CloseAsync(int id, CancellationToken ct = default);
    Task<Result> ReopenAsync(int id, CancellationToken ct = default);
}
