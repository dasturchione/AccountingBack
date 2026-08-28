using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePointOperations;

public interface IPaymentAcceptancePointOperationLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
