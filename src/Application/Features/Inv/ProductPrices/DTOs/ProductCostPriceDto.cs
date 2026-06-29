namespace Application.Features.Inv.ProductPrices;

public class ProductCostPriceDto
{
    public decimal CostPrice { get; set; }

    public List<ProductCostPriceTableDto> Purchases { get; set; } = new();
}

public class ProductCostPriceTableDto
{
    public long PurchaseId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime Date { get; set; }
    public int Quantity { get => ProductTableIds.Count; }
    public decimal UnitPrice { get; set; }
    public List<int> ProductTableIds { get; set; } = new();
}