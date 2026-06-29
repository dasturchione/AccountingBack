namespace Application.Features.Inv.ProductPrices;

public interface IProductPriceCalculateService
{
    Task<Dictionary<int, ProductSalePriceDto>> GetSalePriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default);
    Task<Dictionary<int, ProductCostPriceDto>> GetCostPriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default);
}
