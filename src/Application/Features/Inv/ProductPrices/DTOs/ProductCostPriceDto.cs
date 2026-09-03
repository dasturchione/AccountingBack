namespace Application.Features.Inv.ProductPrices;

public class ProductCostPriceDto
{
    public decimal CostPrice { get; set; }

    public List<ProductCostPriceTableDto> Purchases { get; set; } = new();
}
