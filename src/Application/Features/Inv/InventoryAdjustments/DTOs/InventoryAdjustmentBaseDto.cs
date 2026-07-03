namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentBaseDto
{
    public DateTime DocDate { get; set; }
    public int WarehouseId { get; set; }
    public string AdjustmentType { get; set; } = null!;
    public string? Comment { get; set; }
    public List<InventoryAdjustmentLineRequestDto> Lines { get; set; } = new();
}
