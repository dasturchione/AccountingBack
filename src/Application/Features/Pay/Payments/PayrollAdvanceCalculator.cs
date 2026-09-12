using SharedKernel.Constants;

namespace Application.Features.Pay.Payments;

/// <summary>
/// Computes the per-employee advance (first-half-of-month payment) from the employee's
/// 1C-style advance setting: a percentage of the planned salary (oklad × rate) or a
/// fixed amount. The result is a suggestion the accountant can adjust on the vedomost.
/// </summary>
public static class PayrollAdvanceCalculator
{
    public static decimal Compute(string method, decimal advanceValue, decimal monthlySalary, decimal employmentRate)
    {
        var amount = method switch
        {
            PayrollAdvanceMethodConst.Fixed => advanceValue,
            PayrollAdvanceMethodConst.Percent => monthlySalary * employmentRate * advanceValue / 100m,
            _ => 0m
        };
        return Math.Max(0m, decimal.Round(amount, 2, MidpointRounding.AwayFromZero));
    }
}
