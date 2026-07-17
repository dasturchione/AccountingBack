namespace Application.Features.Inv.WarehouseProducts;

public sealed class WarehouseProductDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public string? ProductMxik { get; init; }
    public int? ProductGroupId { get; init; }
    public string? ProductGroupName { get; init; }
    public short UnitId { get; init; }
    public string UnitName { get; init; } = null!;
    public string UnitCode { get; init; } = null!;
    public bool IsPieceTracked { get; init; }
    public decimal Quantity { get; init; }
    public decimal ReservedQuantity { get; init; }
    public decimal BlockedQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public IReadOnlyList<WarehouseProductBatchDto> Batches { get; init; } = Array.Empty<WarehouseProductBatchDto>();
}

public sealed class WarehouseProductBatchDto
{
    public long BatchId { get; init; }
    public string BatchNumber { get; init; } = null!;
    public DateTime ReceivedDate { get; init; }
    public long DocumentId { get; init; }
    public decimal Quantity { get; init; }
    public decimal ReservedQuantity { get; init; }
    public decimal BlockedQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public decimal UnitCost { get; init; }
}
