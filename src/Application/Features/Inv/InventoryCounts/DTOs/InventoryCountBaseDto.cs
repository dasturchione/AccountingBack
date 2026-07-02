namespace Application.Features.InventoryCounts;

public class InventoryCountBaseDto
{
    public DateTime DocDate { get; set; }
    public int WarehouseId { get; set; }
    public string? Comment { get; set; }
    public bool IsCountCompleted { get; set; }
    public List<InventoryCountLineRequestDto> Lines { get; set; } = new();
}
