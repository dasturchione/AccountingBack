using SharedKernel.Results;

namespace Application.Features.Inv.WarehouseProducts;

public interface IWarehouseInventoryService
{
    Task<Result<IReadOnlyList<WarehouseProductDto>>> GetWarehouseProductsAsync(
        WarehouseProductFilter filter,
        CancellationToken cancellationToken = default);
}
