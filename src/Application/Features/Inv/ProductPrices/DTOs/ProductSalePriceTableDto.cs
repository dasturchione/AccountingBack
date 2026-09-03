namespace Application.Features.Inv.ProductPrices;

public class ProductSalePriceTableDto
{
    public long PurchaseId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity => ProductTableIds.Count;
    public List<int> ProductTableIds { get; set; } = new();
}
