using Application.Features.Pay.Taxes;

namespace UnitTests;

public sealed class PayrollIncrementalTaxTests
{
    [Fact]
    public void WithNoPriorAmounts_BehavesLikeAOneShotCalculation()
    {
        var input = new PayrollTaxCalculationInput(BaseAmount: 2_000_000m, Rate: 12m);

        var oneShot = PayrollTaxCalculator.Calculate(input);
        var incremental = PayrollTaxCalculator.CalculateIncremental(input, PayrollPriorTax.None);

        Assert.Equal(240_000m, oneShot.Amount);
        Assert.Equal(oneShot, incremental);
    }

    [Fact]
    public void Correction_IsTaxedOnItsOwnDelta()
    {
        // The regular run already taxed 2 000 000; the correction adds a 1 000 000 bonus.
        var prior = new PayrollPriorTax(BaseAmount: 2_000_000m, Amount: 240_000m);

        var result = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(BaseAmount: 1_000_000m, Rate: 12m),
            prior);

        Assert.Equal(120_000m, result.Amount);
    }

    [Fact]
    public void MonthlyExemption_IsNotGrantedASecondTime()
    {
        // 12% with a 500 000 exemption. Regular: (2 000 000 - 500 000) * 12% = 180 000.
        var prior = new PayrollPriorTax(BaseAmount: 2_000_000m, Amount: 180_000m);

        var result = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(BaseAmount: 1_000_000m, Rate: 12m, ExemptionAmount: 500_000m),
            prior);

        // The whole 1 000 000 is taxable: the exemption was already used this period.
        Assert.Equal(120_000m, result.Amount);

        // Computing the correction from scratch is what used to hand out a second exemption.
        var fromScratch = PayrollTaxCalculator.Calculate(
            new PayrollTaxCalculationInput(BaseAmount: 1_000_000m, Rate: 12m, ExemptionAmount: 500_000m));
        Assert.Equal(60_000m, fromScratch.Amount);
    }

    [Fact]
    public void TaxableBaseLimit_DoesNotRestartOnACorrection()
    {
        // The limit caps the taxable base at 2 500 000 for the whole period.
        var prior = new PayrollPriorTax(BaseAmount: 2_000_000m, Amount: 240_000m);

        var result = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(BaseAmount: 1_000_000m, Rate: 12m, LimitAmount: 2_500_000m),
            prior);

        // Only 500 000 of the correction is still under the cap.
        Assert.Equal(60_000m, result.Amount);
        Assert.Equal(500_000m, result.TaxableBase);
    }

    [Fact]
    public void NegativeCorrection_ReturnsOverWithheldTax()
    {
        var prior = new PayrollPriorTax(BaseAmount: 2_000_000m, Amount: 240_000m);

        var result = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(BaseAmount: -500_000m, Rate: 12m),
            prior);

        Assert.Equal(-60_000m, result.Amount);
        Assert.Equal(-500_000m, result.TaxableBase);
    }

    [Fact]
    public void CorrectionThatCancelsThePeriod_GivesBackEverythingThatWasWithheld()
    {
        var prior = new PayrollPriorTax(BaseAmount: 2_000_000m, Amount: 240_000m);

        var result = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(BaseAmount: -2_000_000m, Rate: 12m),
            prior);

        Assert.Equal(-240_000m, result.Amount);
    }

    [Fact]
    public void OverstatedNegativeCorrection_NeverRefundsMoreThanWasWithheld()
    {
        var prior = new PayrollPriorTax(BaseAmount: 2_000_000m, Amount: 240_000m);

        var result = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(BaseAmount: -5_000_000m, Rate: 12m),
            prior);

        Assert.Equal(-240_000m, result.Amount);
    }

    [Fact]
    public void ExemptedRegularRun_StillUsesUpTheExemptionForALaterCorrection()
    {
        // The regular run stayed under the exemption, so it withheld nothing, but it did
        // consume the exemption. Its base has to carry into the correction.
        var prior = new PayrollPriorTax(BaseAmount: 300_000m, Amount: 0m);

        var result = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(BaseAmount: 400_000m, Rate: 12m, ExemptionAmount: 500_000m),
            prior);

        // Period-to-date 700 000, of which 200 000 is taxable.
        Assert.Equal(24_000m, result.Amount);
        Assert.Equal(200_000m, result.TaxableBase);
    }

    [Fact]
    public void SuccessiveCorrections_AddUpToTheTaxOnTheWholePeriod()
    {
        var definition = (Rate: 12m, Exemption: 500_000m);

        var regular = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(2_000_000m, definition.Rate, definition.Exemption),
            PayrollPriorTax.None);
        var firstCorrection = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(1_000_000m, definition.Rate, definition.Exemption),
            new PayrollPriorTax(2_000_000m, regular.Amount));
        var secondCorrection = PayrollTaxCalculator.CalculateIncremental(
            new PayrollTaxCalculationInput(400_000m, definition.Rate, definition.Exemption),
            new PayrollPriorTax(3_000_000m, regular.Amount + firstCorrection.Amount));

        var wholePeriod = PayrollTaxCalculator.Calculate(
            new PayrollTaxCalculationInput(3_400_000m, definition.Rate, definition.Exemption));

        Assert.Equal(
            wholePeriod.Amount,
            regular.Amount + firstCorrection.Amount + secondCorrection.Amount);
    }
}
