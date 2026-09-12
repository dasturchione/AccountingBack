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
    public void CalculateSegmented_SingleSegment_EqualsWholePeriodProration()
    {
        var whole = PayrollSalaryProrationCalculator.Calculate(
            monthlySalary: 4_400_000m,
            employmentRate: 1m,
            workedDays: 20m,
            normDays: 22m,
            workedHours: 160m,
            normHours: 176m,
            basis: PayrollProrationBasisConst.Days);

        var segmented = PayrollSalaryProrationCalculator.CalculateSegmented(
            [new PayrollProrationSegment(4_400_000m, 1m, 20m, 22m, 160m, 176m, 0m, 0m, 0m)],
            PayrollProrationBasisConst.Days);

        Assert.Equal(whole, segmented);
    }

    [Fact]
    public void CalculateSegmented_MidPeriodSalaryChange_SumsPerSegmentSalaries()
    {
        // 11 worked days on the old 2,200,000 salary (norm 22) +
        // 11 worked days on the new 4,400,000 salary (norm 22).
        var segmented = PayrollSalaryProrationCalculator.CalculateSegmented(
            [
                new PayrollProrationSegment(2_200_000m, 1m, 11m, 22m, 88m, 176m, 0m, 0m, 0m),
                new PayrollProrationSegment(4_400_000m, 1m, 11m, 22m, 88m, 176m, 0m, 0m, 0m)
            ],
            PayrollProrationBasisConst.Days);

        // 1,100,000 + 2,200,000 — NOT the buggy 2,200,000 (latest salary for all days).
        Assert.Equal(3_300_000m, segmented);
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
