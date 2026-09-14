using Application.Features.RetailSaleDocs;
using SharedKernel.Money;

namespace UnitTests;

public sealed class VatInclusivePricingTests
{
    [Fact]
    public void ExclusivePrice_AddsVatOnTop()
    {
        var money = VatCalculator.FromNet(12_000m, rate: 12m);

        Assert.Equal(12_000m, money.NetAmount);
        Assert.Equal(1_440m, money.VatAmount);
        Assert.Equal(13_440m, money.GrossAmount);
    }

    [Fact]
    public void InclusivePrice_ExtractsVatFromTheShelfPrice()
    {
        // A 12 000 soum shelf price is what the customer hands over.
        var money = VatCalculator.FromGross(12_000m, rate: 12m);

        Assert.Equal(1_285.71m, money.VatAmount);
        Assert.Equal(10_714.29m, money.NetAmount);
        Assert.Equal(12_000m, money.GrossAmount);
    }

    [Fact]
    public void InclusivePrice_LeavesNothingHangingOnTheReceivable()
    {
        // The receivable is debited with net + VAT; the till only ever receives the gross.
        var money = VatCalculator.FromGross(12_000m, rate: 12m);

        Assert.Equal(money.GrossAmount, money.NetAmount + money.VatAmount);
    }

    [Fact]
    public void TreatingAnInclusivePriceAsExclusive_IsWhatUsedToOverstateTheSale()
    {
        var wrong = VatCalculator.FromNet(12_000m, rate: 12m);
        var right = VatCalculator.FromGross(12_000m, rate: 12m);

        Assert.Equal(1_440m, wrong.GrossAmount - right.GrossAmount);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(15)]
    [InlineData(0)]
    public void NetAndVat_AlwaysAddUpToTheGross(int rate)
    {
        var money = VatCalculator.FromGross(999_999.99m, rate);

        Assert.Equal(999_999.99m, money.NetAmount + money.VatAmount);
        Assert.Equal(999_999.99m, money.GrossAmount);
    }

    [Fact]
    public void ZeroRate_LeavesTheWholeAmountAsNet()
    {
        var inclusive = VatCalculator.FromGross(12_000m, rate: 0m);
        var exclusive = VatCalculator.FromNet(12_000m, rate: 0m);

        Assert.Equal(0m, inclusive.VatAmount);
        Assert.Equal(12_000m, inclusive.NetAmount);
        Assert.Equal(inclusive, exclusive);
    }

    [Fact]
    public void Resolve_WithoutARate_TreatsTheWholeAmountAsNet()
    {
        var money = VatCalculator.Resolve(12_000m, rate: null, priceIncludesVat: true);

        Assert.Equal(12_000m, money.NetAmount);
        Assert.Equal(0m, money.VatAmount);
    }

    [Fact]
    public void RetailLine_ReadsTheShelfPriceAsVatInclusive()
    {
        var money = RetailSaleVatCalculator.Resolve(
            unitPrice: 12_000m,
            quantity: 2m,
            vatAmount: null,
            vatRate: 12m,
            priceIncludesVat: true);

        Assert.Equal(24_000m, money.GrossAmount);
        Assert.Equal(2_571.43m, money.VatAmount);
        Assert.Equal(21_428.57m, money.NetAmount);
    }

    [Fact]
    public void RetailLine_WithAFiscalVatAmount_KeepsItAndDerivesTheRest()
    {
        // The fiscal register reported the VAT inside a 24 000 receipt line.
        var money = RetailSaleVatCalculator.Resolve(
            unitPrice: 12_000m,
            quantity: 2m,
            vatAmount: 2_571.43m,
            vatRate: 12m,
            priceIncludesVat: true);

        Assert.Equal(2_571.43m, money.VatAmount);
        Assert.Equal(24_000m, money.GrossAmount);
        Assert.Equal(21_428.57m, money.NetAmount);
    }
}
