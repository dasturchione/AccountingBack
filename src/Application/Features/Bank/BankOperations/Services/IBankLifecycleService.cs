using SharedKernel.Results;

namespace Application.Features.BankOperations;

public interface IBankLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
