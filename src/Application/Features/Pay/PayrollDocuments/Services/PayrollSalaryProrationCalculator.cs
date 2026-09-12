using SharedKernel.Constants;

namespace Application.Features.Pay.PayrollDocuments;

public static class PayrollSalaryProrationCalculator
{
    public static decimal Calculate(
        decimal monthlySalary,
        decimal employmentRate,
        decimal workedDays,
        decimal normDays,
        decimal workedHours,
        decimal normHours,
        string basis,
        decimal paidLeaveDays = 0m,
        decimal paidLeaveHours = 0m,
        decimal paidSickDays = 0m)
    {
        var baseAmount = monthlySalary * employmentRate;
        var fraction = basis switch
        {
            PayrollProrationBasisConst.Days => normDays == 0m ? 0m : (workedDays + paidLeaveDays + paidSickDays) / normDays,
            PayrollProrationBasisConst.Hours => normHours == 0m ? 0m : (workedHours + paidLeaveHours) / normHours,
            _ => throw new ArgumentException($"Unsupported payroll proration basis: '{basis}'.", nameof(basis))
        };

        return decimal.Round(baseAmount * fraction, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Sums the prorated base salary across employment segments. Each segment is
    /// prorated with its own salary, rate and norm, so a mid-period salary or
    /// position change is reflected correctly (1C-style). With a single segment the
    /// result equals <see cref="Calculate"/> over the whole period.
    /// </summary>
    public static decimal CalculateSegmented(IEnumerable<PayrollProrationSegment> segments, string basis)
    {
        ArgumentNullException.ThrowIfNull(segments);
        var total = 0m;
        foreach (var s in segments)
            total += Calculate(
                s.MonthlySalary,
                s.EmploymentRate,
                s.WorkedDays,
                s.NormDays,
                s.WorkedHours,
                s.NormHours,
                basis,
                s.PaidLeaveDays,
                s.PaidLeaveHours,
                s.PaidSickDays);
        return total;
    }
}

public readonly record struct PayrollProrationSegment(
    decimal MonthlySalary,
    decimal EmploymentRate,
    decimal WorkedDays,
    decimal NormDays,
    decimal WorkedHours,
    decimal NormHours,
    decimal PaidLeaveDays,
    decimal PaidLeaveHours,
    decimal PaidSickDays);
