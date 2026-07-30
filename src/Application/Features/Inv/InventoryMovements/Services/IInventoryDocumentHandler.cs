using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public interface IInventoryDocumentHandler<T>
{
    Task<Result<List<InventoryMovementEntry>>> HandleAsync(T document, CancellationToken ct = default);
}
