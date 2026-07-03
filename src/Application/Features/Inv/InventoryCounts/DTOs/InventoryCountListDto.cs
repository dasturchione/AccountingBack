namespace Application.Features.InventoryCounts;

public class InventoryCountListDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? CountCompletedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
