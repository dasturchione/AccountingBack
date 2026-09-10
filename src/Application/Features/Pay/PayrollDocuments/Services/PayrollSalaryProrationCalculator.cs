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
}
