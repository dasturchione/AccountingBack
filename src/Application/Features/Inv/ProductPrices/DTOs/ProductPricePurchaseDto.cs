using Application.Features.Inv.ProductStocks;

namespace Application.Features.ProductPrices;

public class ProductPricePurchaseDto : ProductStockPurchaseDto
{
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
}
