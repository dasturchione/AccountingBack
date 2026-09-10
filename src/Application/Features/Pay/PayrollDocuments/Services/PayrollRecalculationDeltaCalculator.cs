using Domain.Entities;

namespace Application.Features.Pay.PayrollDocuments;

public sealed record PayrollRecalculationLineDelta(
    decimal WorkedDays,
    decimal WorkedHours,
    decimal PaidLeaveDays,
    decimal PaidSickDays,
    decimal OvertimeHours,
    decimal NightHours,
    decimal HolidayHours,
    decimal WeekendHours,
    decimal GrossAmount,
    decimal DeductionAmount,
    decimal EmployerTaxAmount,
    decimal AdvanceAmount,
    decimal NetAmount,
    decimal PayableAmount,
    IReadOnlyDictionary<int, decimal> ComponentDeltas,
    IReadOnlyDictionary<int, decimal> TaxDeltas);

public static class PayrollRecalculationDeltaCalculator
{
    public static PayrollRecalculationLineDelta Calculate(
        PayPayrollLine original,
        PayPayrollLine recalculated)
    {
        var componentDeltas = original.CalcLines
            .Select(x => (x.ComponentId, Amount: -x.Amount))
            .Concat(recalculated.CalcLines.Select(x => (x.ComponentId, x.Amount)))
            .GroupBy(x => x.ComponentId)
            .ToDictionary(x => x.Key, x => Round(x.Sum(y => y.Amount)));

        var taxDeltas = original.TaxLines
            .Select(x => (x.TaxDefinitionId, Amount: -x.Amount))
            .Concat(recalculated.TaxLines.Select(x => (x.TaxDefinitionId, x.Amount)))
            .GroupBy(x => x.TaxDefinitionId)
            .ToDictionary(x => x.Key, x => Round(x.Sum(y => y.Amount)));

        return new PayrollRecalculationLineDelta(
            Round(recalculated.WorkedDays - original.WorkedDays),
            Round(recalculated.WorkedHours - original.WorkedHours),
            Round(recalculated.PaidLeaveDays - original.PaidLeaveDays),
            Round(recalculated.PaidSickDays - original.PaidSickDays),
            Round(recalculated.OvertimeHours - original.OvertimeHours),
            Round(recalculated.NightHours - original.NightHours),
            Round(recalculated.HolidayHours - original.HolidayHours),
            Round(recalculated.WeekendHours - original.WeekendHours),
            Round(recalculated.GrossAmount - original.GrossAmount),
            Round(recalculated.DeductionAmount - original.DeductionAmount),
            Round(recalculated.EmployerTaxAmount - original.EmployerTaxAmount),
            Round(recalculated.AdvanceAmount - original.AdvanceAmount),
            Round(recalculated.NetAmount - original.NetAmount),
            Round(recalculated.PayableAmount - original.PayableAmount),
            componentDeltas,
            taxDeltas);
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
