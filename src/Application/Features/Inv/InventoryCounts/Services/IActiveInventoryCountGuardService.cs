using SharedKernel.Results;

namespace Application.Features.InventoryCounts;

public interface IActiveInventoryCountGuardService
{
    Task<Result> EnsureWarehouseIsNotBlockedAsync(int organizationId, int warehouseId, string operationName, long? currentInventoryCountId = null, CancellationToken ct = default);
}
