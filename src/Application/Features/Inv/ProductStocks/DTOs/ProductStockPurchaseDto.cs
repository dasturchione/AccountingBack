namespace Application.Features.Inv.ProductStocks;

public class ProductStockPurchaseDto
{
    public long PurchaseId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime Date { get; set; }
    public int Qty { get; set; }
    public decimal TotalAmount { get; set; }
    public List<int> ProductTableIds { get; set; } = new();
}
