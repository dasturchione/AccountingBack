namespace Application.Features.Inv.WarehouseProducts;

public sealed class ProductBatchAllocation
{
    public long BatchId { get; init; }
    public decimal Quantity { get; init; }
    public IReadOnlyList<int> ProductTableIds { get; init; } = Array.Empty<int>();
}
