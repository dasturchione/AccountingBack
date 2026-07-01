using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public interface IInventoryDispatcher
{
    Task<Result<List<RegisterBalance>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null);
}
