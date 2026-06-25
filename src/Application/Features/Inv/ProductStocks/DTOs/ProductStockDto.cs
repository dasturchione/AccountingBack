namespace Application.Features.Inv.ProductStocks;

public class ProductStockDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Barcode { get; set; }
    public string? ProductGroupName { get; set; }
    public string UnitName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TotalAmount => Price * Quantity;
}
