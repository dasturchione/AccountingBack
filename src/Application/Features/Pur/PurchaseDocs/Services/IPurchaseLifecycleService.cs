using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public interface IPurchaseLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
