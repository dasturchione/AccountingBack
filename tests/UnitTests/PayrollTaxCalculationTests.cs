using Application.Features.Pay.Taxes;

namespace UnitTests;

public sealed class PayrollTaxCalculationTests
{
    [Fact]
    public void TaxBaseAppliesExemptionAndLimitBeforeRate()
    {
        var result = PayrollTaxCalculator.Calculate(new PayrollTaxCalculationInput(
            BaseAmount: 1_000m,
            Rate: 12m,
            ExemptionAmount: 100m,
            LimitAmount: 800m));

        Assert.Equal(800m, result.TaxableBase);
        Assert.Equal(96m, result.Amount);
    }

    [Fact]
    public void NegativeTaxBaseAndRateAreClampedToZero()
    {
        var result = PayrollTaxCalculator.Calculate(new PayrollTaxCalculationInput(
            BaseAmount: 50m,
            Rate: -5m,
            ExemptionAmount: 100m));

        Assert.Equal(0m, result.TaxableBase);
        Assert.Equal(0m, result.Amount);
    }

    [Fact]
    public void TaxAmountUsesAwayFromZeroRoundingToTwoDecimals()
    {
        var result = PayrollTaxCalculator.Calculate(new PayrollTaxCalculationInput(
            BaseAmount: 10m,
            Rate: 12.345m));

        Assert.Equal(1.23m, result.Amount);
    }
}
