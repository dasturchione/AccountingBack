namespace Application.Features.Inv.ProductStocks;

public class ProductTableByMarkingDto
{
    public int ProductTableId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Mxik { get; set; }
    public string? SerialNumber { get; set; }
    public string? MarkingNumber { get; set; }
    public int? CurrentWarehouseId { get; set; }
    public string? CurrentWarehouseName { get; set; }
}
