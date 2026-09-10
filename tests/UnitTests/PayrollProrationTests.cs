using Application.Features.Pay.PayrollDocuments;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollProrationTests
{
    [Fact]
    public void HourlyBasis_UsesWorkedHoursAgainstEmployeeNormHours()
    {
        var amount = PayrollSalaryProrationCalculator.Calculate(
            monthlySalary: 4_400_000m,
            employmentRate: 1m,
            workedDays: 20m,
            normDays: 22m,
            workedHours: 164m,
            normHours: 176m,
            basis: PayrollProrationBasisConst.Hours);

        Assert.Equal(4_100_000m, amount);
    }

    [Fact]
    public void DayBasis_PreservesExistingDayFormula()
    {
        var amount = PayrollSalaryProrationCalculator.Calculate(
            monthlySalary: 4_400_000m,
            employmentRate: 1m,
            workedDays: 20m,
            normDays: 22m,
            workedHours: 164m,
            normHours: 176m,
            basis: PayrollProrationBasisConst.Days);

        Assert.Equal(4_000_000m, amount);
    }

    [Fact]
    public void HourlyBasis_WithFourHourDay_ReducesSalaryByFourHours()
    {
        var amount = PayrollSalaryProrationCalculator.Calculate(
            monthlySalary: 2_200_000m,
            employmentRate: 1m,
            workedDays: 22m,
            normDays: 22m,
            workedHours: 172m,
            normHours: 176m,
            basis: PayrollProrationBasisConst.Hours);

        Assert.Equal(2_150_000m, amount);
    }

    [Fact]
    public void DayBasis_IncludesPaidLeaveInSalaryTime()
    {
        var amount = PayrollSalaryProrationCalculator.Calculate(
            monthlySalary: 4_400_000m,
            employmentRate: 1m,
            workedDays: 20m,
            normDays: 22m,
            workedHours: 160m,
            normHours: 176m,
            basis: PayrollProrationBasisConst.Days,
            paidLeaveDays: 2m);

        Assert.Equal(4_400_000m, amount);
    }
}
