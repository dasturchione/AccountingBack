namespace Application.Features.Inv.ProductStocks;

public class ProductStockPurchaseDto
{
    public long PurchaseId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal CostPrice { get; set; }
    public List<int> ProductTableIds { get; set; } = new();
}
