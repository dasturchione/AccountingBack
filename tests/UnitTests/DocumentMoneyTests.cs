using Application.Features.RetailSaleDocs;
using SharedKernel.Money;

namespace UnitTests;

public sealed class DocumentMoneyTests
{
    [Fact]
    public void Round_MatchesTheScaleTheRegisterStores()
    {
        Assert.Equal(2, DocumentMoney.Scale);
        Assert.Equal(1_234_567.89m, DocumentMoney.Round(1_234_567.89125m));
        Assert.Equal(0.13m, DocumentMoney.Round(0.125m));
        Assert.Equal(-0.13m, DocumentMoney.Round(-0.125m));
    }

    [Fact]
    public void Vat_IsDecidedAtTwoDecimals_SoThePostingMatchesTheDocument()
    {
        // 3 litres at 4 111.11 with 12% VAT: the eight-decimal result was 1 479.999 600 00,
        // which reached the ledger as 1 480.00 while the document kept the longer number.
        var money = RetailSaleVatCalculator.Resolve(
            unitPrice: 4_111.11m,
            quantity: 3m,
            vatAmount: null,
            vatRate: 12m,
            priceIncludesVat: false);

        Assert.Equal(1_480.00m, money.VatAmount);
        Assert.Equal(money.VatAmount, decimal.Round(money.VatAmount, 2));
    }

    [Fact]
    public void Distribute_KeepsThePartsAddingUpToTheTotal()
    {
        var shares = DocumentMoney.Distribute(100.00m, 3);

        Assert.Equal(3, shares.Count);
        Assert.Equal(100.00m, shares.Sum());
        Assert.Equal([33.34m, 33.33m, 33.33m], shares);
    }

    [Fact]
    public void Distribute_HandsOutOneCentPerShare_NotAllToTheLast()
    {
        var shares = DocumentMoney.Distribute(10.00m, 7);

        Assert.Equal(10.00m, shares.Sum());
        Assert.All(shares, share => Assert.InRange(share, 1.42m, 1.43m));
    }

    [Fact]
    public void Distribute_NegativeTotal_KeepsTheSign()
    {
        var shares = DocumentMoney.Distribute(-100.00m, 3);

        Assert.Equal(-100.00m, shares.Sum());
        Assert.All(shares, share => Assert.True(share < 0m));
    }

    [Fact]
    public void Distribute_ExactDivision_GivesEqualShares()
    {
        var shares = DocumentMoney.Distribute(90.00m, 3);

        Assert.Equal([30.00m, 30.00m, 30.00m], shares);
    }

    [Fact]
    public void Distribute_NoItems_ReturnsNothing()
    {
        Assert.Empty(DocumentMoney.Distribute(100.00m, 0));
    }

    [Theory]
    [InlineData(100.00, 3)]
    [InlineData(0.01, 5)]
    [InlineData(1_234_567.89, 17)]
    [InlineData(0.07, 3)]
    public void Distribute_AlwaysConservesTheTotal(double total, int count)
    {
        var shares = DocumentMoney.Distribute((decimal)total, count);

        Assert.Equal(DocumentMoney.Round((decimal)total), shares.Sum());
        Assert.All(shares, share => Assert.Equal(share, decimal.Round(share, 2)));
    }
}
