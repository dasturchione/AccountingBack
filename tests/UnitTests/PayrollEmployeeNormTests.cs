using Application.Features.Hr.Calendar;
using Application.Features.Pay.Timesheets;
using Domain.Entities;

namespace UnitTests;

public sealed class PayrollEmployeeNormTests
{
    [Fact]
    public void Resolve_UsesEmployeeCalendarNorms_ForNewTimesheetLine()
    {
        var period = new PayPeriod
        {
            NormWorkDays = 22m,
            NormWorkHours = 176m
        };
        var calendar = new HrEmployeeCalendarDto
        {
            Summary = new HrEmployeeCalendarSummaryDto
            {
                NormWorkDays = 20m,
                NormWorkHours = 160m
            }
        };

        var result = PayrollTimesheetNormCalculator.Resolve(period, calendar);

        Assert.Equal(20m, result.NormWorkDays);
        Assert.Equal(160m, result.NormWorkHours);
    }

    [Fact]
    public void Resolve_FallsBackToPeriodNorms_WhenCalendarIsUnavailable()
    {
        var period = new PayPeriod { NormWorkDays = 22m, NormWorkHours = 176m };

        var result = PayrollTimesheetNormCalculator.Resolve(period, null);

        Assert.Equal(22m, result.NormWorkDays);
        Assert.Equal(176m, result.NormWorkHours);
    }

    [Fact]
    public void Resolve_AppliesPeriodWorkDates_ToEmployeeSchedule()
    {
        var period = new PayPeriod { NormWorkDays = 22m, NormWorkHours = 176m };
        var calendar = new HrEmployeeCalendarDto
        {
            Days =
            [
                new HrEmployeeCalendarDayDto { Date = new DateOnly(2026, 9, 1), PlannedHours = 8m },
                new HrEmployeeCalendarDayDto { Date = new DateOnly(2026, 9, 2), PlannedHours = 8m }
            ],
            Summary = new HrEmployeeCalendarSummaryDto { NormWorkDays = 2m, NormWorkHours = 16m }
        };

        var result = PayrollTimesheetNormCalculator.Resolve(
            period,
            calendar,
            periodWorkDates: [new DateOnly(2026, 9, 1)]);

        Assert.Equal(1m, result.NormWorkDays);
        Assert.Equal(8m, result.NormWorkHours);
    }
}
