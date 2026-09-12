using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Pay.PayrollDocuments;

/// <summary>
/// Recomputes payroll line and document money totals from the (possibly admin-edited)
/// calc and tax lines. Used when a DRAFT document is edited: totals are a pure sum of
/// the stored lines, so whatever amounts the accountant sets are reflected consistently
/// on the line and the document header (and therefore in the postings at confirm time).
/// </summary>
public static class PayrollDocumentTotalsCalculator
{
    public static void RecomputeLine(PayPayrollLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var gross = Round(line.CalcLines
            .Where(c => c.Component.ComponentType == PayrollComponentTypeConst.Earning)
            .Sum(c => c.Amount));
        var deductions = Round(
            line.CalcLines
                .Where(c => c.Component.ComponentType == PayrollComponentTypeConst.Deduction)
                .Sum(c => c.Amount)
            + line.TaxLines
                .Where(t => t.TaxDefinition.TaxType == PayrollTaxTypeConst.Withholding)
                .Sum(t => t.Amount));
        var employerTax = Round(
            line.CalcLines
                .Where(c => c.Component.ComponentType == PayrollComponentTypeConst.EmployerTax)
                .Sum(c => c.Amount)
            + line.TaxLines
                .Where(t => t.TaxDefinition.TaxType == PayrollTaxTypeConst.Employer)
                .Sum(t => t.Amount));

        line.GrossAmount = gross;
        line.DeductionAmount = deductions;
        line.EmployerTaxAmount = employerTax;
        line.NetAmount = Round(gross - deductions);
        line.PayableAmount = Round(line.NetAmount - line.AdvanceAmount);
    }

    public static void RecomputeDocument(PayPayrollDoc document)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.GrossAmount = Round(document.Lines.Sum(x => x.GrossAmount));
        document.DeductionAmount = Round(document.Lines.Sum(x => x.DeductionAmount));
        document.EmployerTaxAmount = Round(document.Lines.Sum(x => x.EmployerTaxAmount));
        document.AdvanceAmount = Round(document.Lines.Sum(x => x.AdvanceAmount));
        document.NetAmount = Round(document.Lines.Sum(x => x.NetAmount));
        document.PayableAmount = Round(document.Lines.Sum(x => x.PayableAmount));
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
