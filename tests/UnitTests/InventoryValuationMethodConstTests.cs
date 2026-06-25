using SharedKernel.Constants;

namespace UnitTests;

public class InventoryValuationMethodConstTests
{
    [Fact]
    public void Values_ShouldMatchDatabaseCheckConstraint()
    {
        Assert.Equal("fifo", InventoryValuationMethodConst.FIFO);
        Assert.Equal("lifo", InventoryValuationMethodConst.LIFO);
        Assert.Equal("average", InventoryValuationMethodConst.AVERAGE);
    }
}
