using Application.Features.Inv.WarehouseProducts;

namespace UnitTests;

public sealed class WarehouseProductPublicContractTests
{
    [Fact]
    public void SplitBatchDtoPropertiesRemainStable()
    {
        var actual = typeof(WarehouseProductBatchDto).GetProperties()
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(property => property.Name)
            .ToArray();
        var expected = new (string Name, Type PropertyType)[]
        {
            ("AvailableQuantity", typeof(decimal)),
            ("BatchId", typeof(long)),
            ("BatchNumber", typeof(string)),
            ("BlockedQuantity", typeof(decimal)),
            ("DocumentId", typeof(long)),
            ("Quantity", typeof(decimal)),
            ("ReceivedDate", typeof(DateTime)),
            ("ReservedQuantity", typeof(decimal)),
            ("UnitCost", typeof(decimal))
        };

        Assert.Equal(expected.OrderBy(property => property.Name).ToArray(), actual);
        Assert.Equal(typeof(IReadOnlyList<WarehouseProductBatchDto>),
            typeof(WarehouseProductDto).GetProperty(nameof(WarehouseProductDto.Batches))!.PropertyType);
    }
}
