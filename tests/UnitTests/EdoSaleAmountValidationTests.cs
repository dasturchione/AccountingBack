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
    public void LineMismatchBeyondQuantityBasedMoneyRoundingFails()
    {
        Assert.False(EdoSaleAmountValidation.IsLineInternallyConsistent(3m, 10m, 30.02m, 5m, 35.02m));
        Assert.False(EdoSaleAmountValidation.IsLineInternallyConsistent(3m, 10m, 30m, 5m, 35.02m));
    }

    [Fact]
    public void Confirmed692adLineRoundingDifferencesAreAccepted()
    {
        var lines = new[]
        {
            (Quantity: 3m, UnitPrice: 2619961.67m, NetAmount: 7859885m, VatAmount: 943186.2m, TotalWithVat: 8803071.2m),
            (Quantity: 13m, UnitPrice: 3926887.77m, NetAmount: 51049541m, VatAmount: 6125944.92m, TotalWithVat: 57175485.92m),
            (Quantity: 3m, UnitPrice: 2699236.33m, NetAmount: 8097709m, VatAmount: 971725.08m, TotalWithVat: 9069434.08m),
            (Quantity: 19m, UnitPrice: 924434.65m, NetAmount: 17564258.29m, VatAmount: 2107710.99m, TotalWithVat: 19671969.28m)
        };

        Assert.All(lines, line => Assert.True(EdoSaleAmountValidation.IsLineInternallyConsistent(
            line.Quantity,
            line.UnitPrice,
            line.NetAmount,
            line.VatAmount,
            line.TotalWithVat)));
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

    [Fact]
    public void Confirmed692adAggregateLineRoundingResidualIsAccepted()
    {
        var lines = new[]
        {
            (Quantity: 3m, UnitPrice: 2619961.67m, NetAmount: 7859885m, VatAmount: 943186.2m, TotalWithVat: 8803071.2m),
            (Quantity: 13m, UnitPrice: 3926887.77m, NetAmount: 51049541m, VatAmount: 6125944.92m, TotalWithVat: 57175485.92m),
            (Quantity: 3m, UnitPrice: 2699236.33m, NetAmount: 8097709m, VatAmount: 971725.08m, TotalWithVat: 9069434.08m),
            (Quantity: 19m, UnitPrice: 924434.65m, NetAmount: 17564258.29m, VatAmount: 2107710.99m, TotalWithVat: 19671969.28m)
        };

        var documentNet = lines.Sum(x => x.NetAmount)
            - lines.Sum(x => x.Quantity * x.UnitPrice - x.NetAmount);

        Assert.Equal(0.07m, lines.Sum(x => x.Quantity * x.UnitPrice - x.NetAmount));
        Assert.True(EdoSaleAmountValidation.AreDocumentTotalsConsistent(
            documentNet,
            lines.Sum(x => x.VatAmount),
            lines.Sum(x => x.TotalWithVat),
            lines));
    }

    [Fact]
    public void Confirmed692adDocumentNetDirectMatchIsNotDoubleRounded()
    {
        var lines = new[]
        {
            (Quantity: 3m, UnitPrice: 2619961.67m, NetAmount: 7859885m, VatAmount: 943186.2m, TotalWithVat: 8803071.2m),
            (Quantity: 13m, UnitPrice: 3926887.77m, NetAmount: 51049541m, VatAmount: 6125944.92m, TotalWithVat: 57175485.92m),
            (Quantity: 3m, UnitPrice: 2699236.33m, NetAmount: 8097709m, VatAmount: 971725.08m, TotalWithVat: 9069434.08m),
            (Quantity: 19m, UnitPrice: 924434.65m, NetAmount: 17564258.29m, VatAmount: 2107710.99m, TotalWithVat: 19671969.28m),
            (Quantity: 1m, UnitPrice: 94000035.29m, NetAmount: 94000035.29m, VatAmount: 11280004.23m, TotalWithVat: 105280039.52m)
        };

        Assert.Equal(0.07m, lines.Sum(x => x.Quantity * x.UnitPrice - x.NetAmount));
        Assert.True(EdoSaleAmountValidation.AreDocumentTotalsConsistent(
            178571428.58m,
            21428571.42m,
            200000000.00m,
            lines));
    }

    [Fact]
    public void UnrelatedAggregateNetMismatchIsRejectedEvenWhenLinesAreValid()
    {
        var lines = new[]
        {
            (Quantity: 3m, UnitPrice: 10m, NetAmount: 30m, VatAmount: 5m, TotalWithVat: 35m),
            (Quantity: 2m, UnitPrice: 12.5m, NetAmount: 25m, VatAmount: 4m, TotalWithVat: 29m)
        };

        Assert.False(EdoSaleAmountValidation.AreDocumentTotalsConsistent(
            documentNetAmount: 55.01m,
            documentVatAmount: 9m,
            documentTotalAmount: 64m,
            lines));
    }

    [Fact]
    public void AggregateVatAndTotalMismatchesRemainStrict()
    {
        var lines = new[]
        {
            (Quantity: 3m, UnitPrice: 10m, NetAmount: 30m, VatAmount: 5m, TotalWithVat: 35m),
            (Quantity: 2m, UnitPrice: 12.5m, NetAmount: 25m, VatAmount: 4m, TotalWithVat: 29m)
        };

        Assert.False(EdoSaleAmountValidation.AreDocumentTotalsConsistent(55m, 9.01m, 64m, lines));
        Assert.False(EdoSaleAmountValidation.AreDocumentTotalsConsistent(55m, 9m, 64.01m, lines));
    }

    [Fact]
    public void ClassifiesLineTotalsMismatchSeparately()
    {
        var code = EdoSaleAmountValidation.GetDocumentTotalsFailureCode(
            35m,
            5m,
            35m,
            [(
                Number: 7,
                Quantity: 1m,
                UnitPrice: 30m,
                NetAmount: 30.01m,
                VatAmount: 5m,
                TotalWithVat: 35.01m)]);

        Assert.Equal(EdoSaleAmountValidation.LineTotalsMismatchCode, code);
    }

    [Fact]
    public void ClassifiesAggregateTotalsMismatchSeparately()
    {
        var code = EdoSaleAmountValidation.GetDocumentTotalsFailureCode(
            31m,
            5m,
            36m,
            [(
                Number: 7,
                Quantity: 1m,
                UnitPrice: 30m,
                NetAmount: 30m,
                VatAmount: 5m,
                TotalWithVat: 35m)]);

        Assert.Equal(EdoSaleAmountValidation.AggregateTotalsMismatchCode, code);
    }
}
