using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public interface IInventoryDispatcher
{
    Task<Result<List<InventoryMovementEntry>>> ProcessAsync(
        object document,
        CancellationToken ct = default,
        long? postingBatchId = null);

    Task<Result> ReverseAsync(object document, CancellationToken ct = default);
}
