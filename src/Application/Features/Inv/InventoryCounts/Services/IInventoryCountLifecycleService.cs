using SharedKernel.Results;

namespace Application.Features.InventoryCounts;

public interface IInventoryCountLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
