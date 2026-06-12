using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public interface IInventoryDocumentHandler<T>
{
    Task<Result<List<RegisterBalance>>> HandleAsync(T document, CancellationToken ct = default);
}
