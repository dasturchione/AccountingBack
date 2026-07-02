using SharedKernel.Results;

namespace Application.Features.WarehouseTransfers;

public interface IWarehouseTransferLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
