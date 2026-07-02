namespace Application.Features.InventoryCounts;

public class InventoryCountLineRequestDto
{
    public int ProductId { get; set; }
    public short UnitId { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal DefaultCostPrice { get; set; }
    public string? Comment { get; set; }
    public List<InventoryCountTableRequestDto> Items { get; set; } = new();
}

public class InventoryCountTableRequestDto
{
    public int? ProductTableId { get; set; }
    public string? Barcode { get; set; }
    public string? SerialNumber { get; set; }
    public string? MarkingNumber { get; set; }
    public decimal CostPrice { get; set; }
}
