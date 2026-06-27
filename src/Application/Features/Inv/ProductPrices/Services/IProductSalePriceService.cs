namespace Application.Features.ProductPrices;

public interface IProductSalePriceService
{
    Task<Dictionary<int, decimal>> GetSalePriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default);
}
