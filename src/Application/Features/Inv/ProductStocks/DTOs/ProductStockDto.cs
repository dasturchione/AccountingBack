namespace Application.Features.Inv.ProductStocks;

public class ProductStockDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Barcode { get; set; }
    public string? Mxik { get; set; }
    public bool IsPieceTracked { get; set; }
    public string? ProductGroupName { get; set; }
    public short UnitId { get; set; } 
    public string UnitCode { get; set; } = null!;
    public string UnitName { get; set; } = null!;
    public bool IsService { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TotalAmount => Price * Quantity;
}
