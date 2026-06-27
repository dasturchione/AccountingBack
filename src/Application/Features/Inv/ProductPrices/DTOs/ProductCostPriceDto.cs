using Application.Features.Inv.ProductStocks;

namespace Application.Features.ProductPrices;

public class ProductCostPriceDto
{
    public decimal CostPrice { get; set; }

    public List<ProductStockPurchaseDto> Purchases { get; set; } = new();
}
