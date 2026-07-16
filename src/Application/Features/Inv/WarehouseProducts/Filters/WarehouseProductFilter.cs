namespace Application.Features.Inv.WarehouseProducts;

public sealed class WarehouseProductFilter
{
    public int WarehouseId { get; init; }
    public int? ProductGroupId { get; init; }
    public IReadOnlyCollection<int>? ProductIds { get; init; }
}
