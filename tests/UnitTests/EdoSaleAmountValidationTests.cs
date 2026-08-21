using Application.Features.SaleDocs.EdoSalePreflight;

public sealed class EdoSaleAmountValidationTests
{
    [Fact]
    public void ProviderVatRoundingDoesNotNeedToMatchNetTimesRate()
    {
        var result = EdoSaleAmountValidation.IsLineInternallyConsistent(
            quantity: 3m,
            unitPrice: 10m,
            netAmount: 30m,
            vatAmount: 5.01m,
            totalWithVat: 35.01m);

        Assert.True(result);
    }

    [Fact]
    public void LineNetOrTotalMismatchFails()
    {
        Assert.False(EdoSaleAmountValidation.IsLineInternallyConsistent(3m, 10m, 30.01m, 5m, 35.01m));
        Assert.False(EdoSaleAmountValidation.IsLineInternallyConsistent(3m, 10m, 30m, 5m, 35.02m));
    }

    [Fact]
    public void DecimalRoundingToleranceIsMinimalAndDeterministic()
    {
        Assert.True(EdoSaleAmountValidation.AreEqual(10m, 10.00000001m));
        Assert.False(EdoSaleAmountValidation.AreEqual(10m, 10.00000002m));
    }

    [Fact]
    public void DocumentAggregateMismatchFails()
    {
        var lines = new[]
        {
            (NetAmount: 30m, VatAmount: 5m, TotalWithVat: 35m),
            (NetAmount: 10m, VatAmount: 2m, TotalWithVat: 12m)
        };

        Assert.True(EdoSaleAmountValidation.AreDocumentTotalsConsistent(40m, 7m, 47m, lines));
        Assert.False(EdoSaleAmountValidation.AreDocumentTotalsConsistent(41m, 7m, 48m, lines));
    }
}
