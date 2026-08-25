namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public string AdjustmentType { get; set; } = null!;
    public short DirectionId { get; set; }
    public string DirectionName { get; set; } = null!;
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Comment { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public List<InventoryAdjustmentLineDto> Lines { get; set; } = new();
}

public class InventoryAdjustmentLineDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public string? Comment { get; set; }
    public List<InventoryAdjustmentTableDto> Items { get; set; } = new();
}

public class InventoryAdjustmentTableDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int? ProductTableId { get; set; }
    public decimal CostPrice { get; set; }
    public string? MarkingNumber { get; set; }
    public string? SerialNumber { get; set; }
    public bool WasCreated { get; set; }
}
