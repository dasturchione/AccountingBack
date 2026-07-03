namespace Application.Features.InventoryCounts;

public class InventoryCountDto
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
    public string? Comment { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? CountCompletedAt { get; set; }
    public int? CountCompletedByUserId { get; set; }
    public long? PositiveAdjustmentDocId { get; set; }
    public long? NegativeAdjustmentDocId { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public List<InventoryCountLineDto> Lines { get; set; } = new();
}

public class InventoryCountLineDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal CountedQuantity { get; set; }
    public decimal DefaultCostPrice { get; set; }
    public string? Comment { get; set; }
    public List<InventoryCountTableDto> Items { get; set; } = new();
}

public class InventoryCountTableDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int? ProductTableId { get; set; }
    public string? Barcode { get; set; }
    public string? SerialNumber { get; set; }
    public string? MarkingNumber { get; set; }
    public decimal CostPrice { get; set; }
}
