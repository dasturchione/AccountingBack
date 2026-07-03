using SharedKernel.Results;

namespace Application.Features.InventoryAdjustments;

public interface IInventoryAdjustmentLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
