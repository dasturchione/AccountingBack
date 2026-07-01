using SharedKernel.Results;

namespace Application.Features.CashOperations;

public interface ICashLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
