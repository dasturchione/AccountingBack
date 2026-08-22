using Application.Features.RetailSaleDocs;

namespace UnitTests;

public sealed class RetailSaleVatCalculatorTests
{
    [Fact]
    public void ExplicitVatAmount_IsUsedAsPerUnitValue()
    {
        var result = RetailSaleVatCalculator.ResolvePerUnit(3_750_000m, 450_000m, 12m);

        Assert.Equal(450_000m, result);
    }

    [Fact]
    public void MissingVatAmount_IsCalculatedFromVatRate()
    {
        var result = RetailSaleVatCalculator.ResolvePerUnit(3_750_000m, null, 12m);

        Assert.Equal(450_000m, result);
    }

    [Fact]
    public void MissingVatAmountAndRate_ReturnsZero()
    {
        var result = RetailSaleVatCalculator.ResolvePerUnit(3_750_000m, null, null);

        Assert.Equal(0m, result);
    }
}
