using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public interface IInventoryDispatcher
{
    Task<Result<List<InventoryRegisterBalance>>> ProcessAsync(object document, CancellationToken ct = default);
}
