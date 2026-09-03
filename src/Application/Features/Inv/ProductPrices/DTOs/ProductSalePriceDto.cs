namespace Application.Features.Inv.ProductPrices;

public class ProductSalePriceDto
{
    public decimal SalePrice { get; set; }
    public List<ProductSalePriceTableDto> SalePrices { get; set; } = new();
}
