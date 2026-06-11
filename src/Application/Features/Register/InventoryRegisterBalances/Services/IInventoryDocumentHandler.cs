using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public interface IInventoryDocumentHandler<T>
{
    Task<Result<List<InventoryRegisterBalance>>> HandleAsync(T document, CancellationToken ct = default);
}
