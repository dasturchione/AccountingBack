namespace Application.Features.Inv.ProductPrices;

public class ProductPriceDetailsDto
{
    public ProductSalePriceDto Sale { get; set; } = new();

    public ProductCostPriceDto Cost { get; set; } = new();
}
