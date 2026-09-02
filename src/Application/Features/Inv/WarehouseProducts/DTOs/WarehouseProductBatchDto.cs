namespace Application.Features.Inv.WarehouseProducts;

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
