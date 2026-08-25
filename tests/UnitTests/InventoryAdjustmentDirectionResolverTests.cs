using Application.Features.InventoryAdjustments;
using Domain.Entities;

public sealed class InventoryAdjustmentDirectionResolverTests
{
    [Fact]
    public void FoundStockUsesInDirectionIdRatherThanAdjustmentTypeId()
    {
        var lookup = new MovementDirection
        {
            Id = 1,
            Code = "IN"
        };

        var resolved = InventoryAdjustmentDirectionResolver.TryResolve("found_stock", lookup, out var directionId);

        Assert.True(resolved);
        Assert.Equal(1, directionId);
        Assert.NotEqual(6, directionId);
    }

    [Fact]
    public void FoundStockCannotUseOutDirection()
    {
        var lookup = new MovementDirection
        {
            Id = -1,
            Code = "OUT"
        };

        Assert.False(InventoryAdjustmentDirectionResolver.TryResolve("FOUND_STOCK", lookup, out _));
    }
}
