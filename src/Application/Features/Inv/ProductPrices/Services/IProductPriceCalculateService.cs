namespace Application.Features.ProductPrices;

public interface IProductPriceCalculateService
{
    Task<Dictionary<int, decimal>> GetSalePriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default);
    Task<Dictionary<int, ProductCostPriceDto>> GetCostPriceDetailsMapAsync(IEnumerable<int> productIds, CancellationToken ct = default);
}
