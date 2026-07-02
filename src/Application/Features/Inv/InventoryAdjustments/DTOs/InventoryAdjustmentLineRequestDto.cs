namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentLineRequestDto
{
    public int ProductId { get; set; }
    public short UnitId { get; set; }
    public decimal Quantity { get; set; }
    public string? Comment { get; set; }
    public List<InventoryAdjustmentTableRequestDto> Items { get; set; } = new();
}

public class InventoryAdjustmentTableRequestDto
{
    public int? ProductTableId { get; set; }
    public decimal CostPrice { get; set; }
}
