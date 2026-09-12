using Application.Features.Pay.PayrollDocuments;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollDocumentTotalsTests
{
    [Fact]
    public void RecomputeLine_SumsEarningsDeductionsTaxesAndAdvance()
    {
        var line = new PayPayrollLine
        {
            AdvanceAmount = 100m,
            CalcLines =
            [
                Calc(PayrollComponentTypeConst.Earning, 1_000m),
                Calc(PayrollComponentTypeConst.Earning, 200m),
                Calc(PayrollComponentTypeConst.Deduction, 50m),
                Calc(PayrollComponentTypeConst.EmployerTax, 120m)
            ],
            TaxLines =
            [
                Tax(PayrollTaxTypeConst.Withholding, 144m),
                Tax(PayrollTaxTypeConst.Employer, 30m)
            ]
        };

        PayrollDocumentTotalsCalculator.RecomputeLine(line);

        Assert.Equal(1_200m, line.GrossAmount);
        Assert.Equal(194m, line.DeductionAmount);   // 50 deduction + 144 withholding tax
        Assert.Equal(150m, line.EmployerTaxAmount);  // 120 employer component + 30 employer tax
        Assert.Equal(1_006m, line.NetAmount);        // gross - deductions
        Assert.Equal(906m, line.PayableAmount);      // net - advance
    }

    [Fact]
    public void RecomputeLine_AfterEditingAComponentAmount_ReflectsNewTotals()
    {
        var earning = Calc(PayrollComponentTypeConst.Earning, 1_000m);
        var line = new PayPayrollLine { CalcLines = [earning] };
        PayrollDocumentTotalsCalculator.RecomputeLine(line);
        Assert.Equal(1_000m, line.GrossAmount);

        // Admin edits the earning amount on the draft.
        earning.Amount = 1_500m;
        PayrollDocumentTotalsCalculator.RecomputeLine(line);

        Assert.Equal(1_500m, line.GrossAmount);
        Assert.Equal(1_500m, line.NetAmount);
        Assert.Equal(1_500m, line.PayableAmount);
    }

    [Fact]
    public void RecomputeDocument_SumsLineTotals()
    {
        var document = new PayPayrollDoc
        {
            Lines =
            [
                new PayPayrollLine { GrossAmount = 1_000m, DeductionAmount = 100m, NetAmount = 900m, PayableAmount = 900m },
                new PayPayrollLine { GrossAmount = 500m, DeductionAmount = 50m, NetAmount = 450m, PayableAmount = 450m }
            ]
        };

        PayrollDocumentTotalsCalculator.RecomputeDocument(document);

        Assert.Equal(1_500m, document.GrossAmount);
        Assert.Equal(150m, document.DeductionAmount);
        Assert.Equal(1_350m, document.NetAmount);
        Assert.Equal(1_350m, document.PayableAmount);
    }

    private static PayPayrollCalcLine Calc(string componentType, decimal amount) => new()
    {
        Amount = amount,
        Component = new PayComponent { ComponentType = componentType }
    };

    private static PayPayrollTaxLine Tax(string taxType, decimal amount) => new()
    {
        Amount = amount,
        TaxDefinition = new PayTaxDefinition { TaxType = taxType }
    };
}
