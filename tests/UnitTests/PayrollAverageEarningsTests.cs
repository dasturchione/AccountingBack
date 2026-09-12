using Application.Features.Pay.PayrollDocuments;

namespace UnitTests;

public sealed class PayrollAverageEarningsTests
{
    [Fact]
    public void DailyAverage_DividesGrossByWorkedDays()
    {
        // 60,000,000 over 250 worked days = 240,000/day.
        Assert.Equal(240_000m, PayrollAverageEarningsCalculator.DailyAverage(60_000_000m, 250m));
    }

    [Fact]
    public void DailyAverage_NoHistory_ReturnsZero()
    {
        Assert.Equal(0m, PayrollAverageEarningsCalculator.DailyAverage(0m, 0m));
    }

    [Fact]
    public void Benefit_Leave_FullPercent()
    {
        // Vacation: 240,000/day × 5 days × 100%.
        Assert.Equal(1_200_000m, PayrollAverageEarningsCalculator.Benefit(240_000m, 5m, 100m));
    }

    [Fact]
    public void Benefit_Sick_AppliesPercent()
    {
        // Sick: 240,000/day × 3 days × 60%.
        Assert.Equal(432_000m, PayrollAverageEarningsCalculator.Benefit(240_000m, 3m, 60m));
    }
}
