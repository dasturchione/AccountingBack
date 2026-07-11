using SharedKernel.Results;

namespace Application.Features.Inv.ProductStocks;

public interface IProductStockCalculateService
{
    Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetProductGroupsAsync(
        int? organizationId,
        int? warehouseId = null,
        DateOnly? choosedDate = null,
        IEnumerable<int>? productIds = null,
        CancellationToken ct = default);

    Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetProductsAsync(
        int? organizationId,
        int? warehouseId = null,
        int? productGroupId = null,
        DateOnly? choosedDate = null,
        IEnumerable<int>? productIds = null,
        CancellationToken ct = default);

    Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetProductTablesAsync(
        int? organizationId,
        int? warehouseId = null,
        int? productGroupId = null,
        DateOnly? choosedDate = null,
        IEnumerable<int>? productIds = null,
        CancellationToken ct = default);
}
