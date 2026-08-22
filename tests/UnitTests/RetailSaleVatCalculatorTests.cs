using Application.Features.RetailSaleDocs;

namespace UnitTests;

public sealed class RetailSaleVatCalculatorTests
{
    [Fact]
    public void ExplicitVatAmount_IsUsedAsLineTotal()
    {
        var result = RetailSaleVatCalculator.ResolveTotal(375_000m, 10m, 450_000m, 12m);

        Assert.Equal(450_000m, result);
    }

    [Fact]
    public void MissingVatAmount_IsCalculatedFromVatRate()
    {
        var result = RetailSaleVatCalculator.ResolveTotal(375_000m, 10m, null, 12m);

        Assert.Equal(450_000m, result);
    }

    [Fact]
    public void MissingVatAmountAndRate_ReturnsZero()
    {
        var result = RetailSaleVatCalculator.ResolveTotal(375_000m, 10m, null, null);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void TotalVatAmount_IsDistributedPerUnitForTrackedItems()
    {
        var result = RetailSaleVatCalculator.ResolvePerUnit(450_000m, 10m);

        Assert.Equal(45_000m, result);
    }
}
