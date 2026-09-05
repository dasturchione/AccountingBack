using Application.Features.Rnt.RentalAccruals;

namespace UnitTests.Features.Rnt;

public sealed class RentalAccrualScheduleTests
{
    [Fact]
    public void GetPeriodForMonthProratesFirstCalendarMonthByContractMonthLength()
    {
        var period = RentalAccrualSchedule.GetPeriodForMonth(
            RentalAccrualSchedule.GetMonth(2024, 7),
            "MONTH",
            new DateTime(2024, 7, 9),
            new DateTime(2024, 10, 9));

        Assert.NotNull(period);
        Assert.Equal(new DateTime(2024, 7, 9), period.Value.PeriodFrom);
        Assert.Equal(new DateTime(2024, 7, 31), period.Value.PeriodTo);
        Assert.Equal(new DateTime(2024, 8, 1), period.Value.NextAccrualDate);
        Assert.Equal(348709.68m, RentalAccrualSchedule.ProrateAmount(470000m, period.Value));
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
    public void GetPeriodForMonthProratesPeriodEndingOnTerminationDate()
    {
        var period = RentalAccrualSchedule.GetPeriodForMonth(
            RentalAccrualSchedule.GetMonth(2026, 9),
            "MONTH",
            new DateTime(2026, 9, 9),
            new DateTime(2026, 9, 20));

        Assert.NotNull(period);
        Assert.Equal(new DateTime(2026, 9, 20), period.Value.PeriodTo);
        Assert.Equal(0.4m, period.Value.ProrationFactor);
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
    public void CalculateContractTotalsUsesFullContractMonthsForIjaraPeriod()
    {
        var result = RentalAccrualSchedule.CalculateContractTotals(
            periodAmount: 3700000m,
            taxBaseAmount: 3700000m,
            taxRate: 0m,
            periodUnit: "MONTH",
            startDate: new DateTime(2026, 2, 20),
            contractEndDate: new DateTime(2026, 5, 19),
            objectEndDate: null,
            terminationDate: null);

        Assert.Equal(11100000m, result.ContractAmount);
        Assert.Equal(11100000m, result.ContractTaxBaseAmount);
        Assert.Equal(0m, result.ContractTaxAmount);
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
