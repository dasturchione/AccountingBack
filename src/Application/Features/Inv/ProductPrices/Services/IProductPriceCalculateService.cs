using SharedKernel.Results;

namespace Application.Features.Inv.ProductPrices;

public interface IProductPriceCalculateService
{
    Task<Dictionary<int, ProductSalePriceDto>> GetSalePriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default);
    Task<Dictionary<int, ProductCostPriceDto>> GetCostPriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default);
    Task<Result<List<ProductTableSelectionDto>>> SelectInventoryAsync(
        int organizationId,
        int warehouseId,
        IReadOnlyCollection<ProductTableSelectionRequestDto> productLines,
        IReadOnlyCollection<int> selectedProductTableIds,
        CancellationToken ct = default);
}
