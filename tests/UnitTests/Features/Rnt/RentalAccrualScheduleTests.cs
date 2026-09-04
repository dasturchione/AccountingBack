using Application.Features.Rnt.RentalAccruals;

namespace UnitTests.Features.Rnt;

public sealed class RentalAccrualScheduleTests
{
    [Fact]
    public void GetPeriodProratesFirstMonthFromContractStartToCalendarMonthEnd()
    {
        var period = RentalAccrualSchedule.GetPeriod(
            new DateTime(2024, 7, 9),
            "MONTH",
            new DateTime(2024, 10, 9));

        Assert.Equal(new DateTime(2024, 7, 9), period.PeriodFrom);
        Assert.Equal(new DateTime(2024, 7, 31), period.PeriodTo);
        Assert.Equal(new DateTime(2024, 8, 1), period.NextAccrualDate);
        Assert.Equal(23m / 31m, period.ProrationFactor);
    }

    [Fact]
    public void GetPreviousMonthHandlesYearBoundary()
    {
        var result = RentalAccrualSchedule.GetPreviousMonth(new DateTime(2025, 1, 1));

        Assert.Equal(2024, result.Year);
        Assert.Equal(12, result.Month);
        Assert.Equal(new DateTime(2024, 12, 1), result.StartDate);
        Assert.Equal(new DateTime(2024, 12, 31), result.EndDate);
    }

    [Fact]
    public void GetMonthSupportsMaximumValidYearAndMonth()
    {
        var result = RentalAccrualSchedule.GetMonth(9999, 12);

        Assert.Equal(new DateTime(9999, 12, 1), result.StartDate);
        Assert.Equal(DateTime.MaxValue.Date, result.EndDate);
    }

    [Fact]
    public void GetPeriodForMaximumMonthDoesNotOverflowNextAccrualDate()
    {
        var result = RentalAccrualSchedule.GetPeriodForMonth(
            RentalAccrualSchedule.GetMonth(9999, 12),
            "MONTH",
            new DateTime(9999, 12, 1),
            DateTime.MaxValue.Date);

        Assert.NotNull(result);
        Assert.Equal(DateTime.MaxValue.Date, result.Value.NextAccrualDate);
    }

    [Fact]
    public void GetPeriodForMonthReturnsOnlySelectedCalendarMonth()
    {
        var result = RentalAccrualSchedule.GetPeriodForMonth(
            RentalAccrualSchedule.GetMonth(2024, 8),
            "MONTH",
            new DateTime(2024, 7, 9),
            new DateTime(2024, 10, 9));

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2024, 8, 1), result.Value.PeriodFrom);
        Assert.Equal(new DateTime(2024, 8, 31), result.Value.PeriodTo);
        Assert.Equal(1m, result.Value.ProrationFactor);
    }

    [Fact]
    public void GetPeriodForMonthUsesActiveDayCountForDailyRent()
    {
        var result = RentalAccrualSchedule.GetPeriodForMonth(
            RentalAccrualSchedule.GetMonth(2024, 7),
            "DAY",
            new DateTime(2024, 7, 9),
            new DateTime(2024, 10, 9));

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2024, 7, 9), result.Value.PeriodFrom);
        Assert.Equal(new DateTime(2024, 7, 31), result.Value.PeriodTo);
        Assert.Equal(23m, result.Value.ProrationFactor);
    }

    [Fact]
    public void GetAccrualLimitUsesTerminationDateBeforePlannedEndDate()
    {
        var result = RentalAccrualSchedule.GetAccrualLimit(
            new DateTime(2026, 12, 31),
            new DateTime(2026, 12, 31),
            new DateTime(2026, 9, 20));

        Assert.Equal(new DateTime(2026, 9, 20), result);
    }

    [Fact]
    public void GetAccrualLimitDoesNotTruncateCurrentPeriodForIndefiniteContract()
    {
        var result = RentalAccrualSchedule.GetAccrualLimit(
            contractEndDate: null,
            objectEndDate: null,
            terminationDate: null);

        Assert.Equal(DateTime.MaxValue.Date, result);
    }

    [Fact]
    public void GetPeriodProratesPeriodEndingOnTerminationDate()
    {
        var period = RentalAccrualSchedule.GetPeriod(
            new DateTime(2026, 9, 9),
            "MONTH",
            new DateTime(2026, 9, 20));

        Assert.Equal(new DateTime(2026, 9, 20), period.PeriodTo);
        Assert.Equal(0.4m, period.ProrationFactor);
    }

    [Fact]
    public void CalculateContractTotalsUsesPeriodAmountsAndProratesFinalPeriod()
    {
        var result = RentalAccrualSchedule.CalculateContractTotals(
            periodAmount: 470000m,
            taxBaseAmount: 600000m,
            taxRate: 12m,
            periodUnit: "MONTH",
            startDate: new DateTime(2024, 7, 9),
            contractEndDate: new DateTime(2024, 10, 9),
            objectEndDate: null,
            terminationDate: null);

        Assert.Equal(1425161.29m, result.ContractAmount);
        Assert.Equal(1819354.84m, result.ContractTaxBaseAmount);
        Assert.Equal(218322.5808m, result.ContractTaxAmount);
    }

    [Fact]
    public void CalculateContractTotalsReturnsNullAmountsForIndefiniteContract()
    {
        var result = RentalAccrualSchedule.CalculateContractTotals(
            periodAmount: 470000m,
            taxBaseAmount: 600000m,
            taxRate: 12m,
            periodUnit: "MONTH",
            startDate: new DateTime(2024, 7, 9),
            contractEndDate: null,
            objectEndDate: null,
            terminationDate: null);

        Assert.Null(result.ContractAmount);
        Assert.Null(result.ContractTaxBaseAmount);
        Assert.Null(result.ContractTaxAmount);
    }
}
