using Application.Features.Pay.Payments;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollAdvanceCalculatorTests
{
    [Fact]
    public void Percent_TakesShareOfPlannedSalary()
    {
        // 40% of 5,000,000 × rate 1.0 = 2,000,000.
        var amount = PayrollAdvanceCalculator.Compute(
            PayrollAdvanceMethodConst.Percent, advanceValue: 40m, monthlySalary: 5_000_000m, employmentRate: 1m);

        Assert.Equal(2_000_000m, amount);
    }

    [Fact]
    public void Percent_HonoursEmploymentRate()
    {
        // 50% of 4,000,000 × rate 0.5 = 1,000,000.
        var amount = PayrollAdvanceCalculator.Compute(
            PayrollAdvanceMethodConst.Percent, advanceValue: 50m, monthlySalary: 4_000_000m, employmentRate: 0.5m);

        Assert.Equal(1_000_000m, amount);
    }

    [Fact]
    public void Fixed_ReturnsTheConfiguredAmount()
    {
        var amount = PayrollAdvanceCalculator.Compute(
            PayrollAdvanceMethodConst.Fixed, advanceValue: 1_500_000m, monthlySalary: 9_000_000m, employmentRate: 1m);

        Assert.Equal(1_500_000m, amount);
    }

    [Fact]
    public void ZeroValue_ProducesNoAdvance()
    {
        var amount = PayrollAdvanceCalculator.Compute(
            PayrollAdvanceMethodConst.Percent, advanceValue: 0m, monthlySalary: 5_000_000m, employmentRate: 1m);

        Assert.Equal(0m, amount);
    }
}
