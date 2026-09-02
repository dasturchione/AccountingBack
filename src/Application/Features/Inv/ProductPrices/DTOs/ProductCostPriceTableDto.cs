namespace Application.Features.Inv.ProductPrices;

public class ProductCostPriceTableDto
{
    public long PurchaseId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime Date { get; set; }
    public int Quantity => ProductTableIds.Count;
    public decimal UnitPrice { get; set; }
    public List<int> ProductTableIds { get; set; } = new();
}
