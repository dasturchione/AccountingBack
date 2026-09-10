using Application.Features.Pay.Periods;
using Application.Features.Hr.Calendar;
using Application.Features.Pay.Timesheets;
using Application.Features.Pay.Validators;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollPeriodCalendarTests
{
    [Fact]
    public void Calculate_DeduplicatesSortsAndDerivesTotals()
    {
        var result = PayrollPeriodCalendarCalculator.Calculate(
            2026,
            9,
            8m,
            [new(2026, 9, 3), new(2026, 9, 1), new(2026, 9, 3)]);

        Assert.Equal(
            [new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)],
            result.WorkDates);
        Assert.Equal(2m, result.NormWorkDays);
        Assert.Equal(16m, result.NormWorkHours);
    }

    [Fact]
    public void Calculate_UsesPerDateTypeAndHours_ForPeriodCalendar()
    {
        var result = PayrollPeriodCalendarCalculator.CalculateFromCalendarDays(
            2026,
            9,
            8m,
            [
                new PayrollPeriodCalendarDaySaveDto
                {
                    Date = new DateOnly(2026, 9, 1),
                    DayType = PayrollPeriodDayTypeConst.Normal,
                    WorkHours = 8m
                },
                new PayrollPeriodCalendarDaySaveDto
                {
                    Date = new DateOnly(2026, 9, 2),
                    DayType = PayrollPeriodDayTypeConst.Holiday,
                    WorkHours = 0m
                },
                new PayrollPeriodCalendarDaySaveDto
                {
                    Date = new DateOnly(2026, 9, 3),
                    DayType = PayrollPeriodDayTypeConst.Shortened,
                    WorkHours = 4m
                }
            ]);

        Assert.Equal([new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)], result.WorkDates);
        Assert.Equal(2m, result.NormWorkDays);
        Assert.Equal(12m, result.NormWorkHours);
        Assert.Equal(4m, result.CalendarDays.Single(day => day.Date == new DateOnly(2026, 9, 3)).WorkHours);
    }

    [Fact]
    public void Overlay_AppliesHolidayAndShortenedHoursToEmployeeCalendar()
    {
        var calendar = new HrEmployeeCalendarDto
        {
            EmployeeId = 1,
            Days =
            [
                new HrEmployeeCalendarDayDto
                {
                    Date = new DateOnly(2026, 9, 1),
                    StatusCode = HrCalendarStatusConst.Worked,
                    PlannedHours = 8m,
                    WorkedHours = 8m
                },
                new HrEmployeeCalendarDayDto
                {
                    Date = new DateOnly(2026, 9, 2),
                    StatusCode = HrCalendarStatusConst.Worked,
                    PlannedHours = 8m,
                    WorkedHours = 8m
                },
                new HrEmployeeCalendarDayDto
                {
                    Date = new DateOnly(2026, 9, 3),
                    StatusCode = HrCalendarStatusConst.DayOff
                }
            ]
        };
        var period = new PayPeriod
        {
            WorkDays =
            [
                new PayPeriodWorkDay
                {
                    WorkDate = new DateOnly(2026, 9, 1),
                    IsWorkDay = true,
                    WorkHours = 8m
                },
                new PayPeriodWorkDay
                {
                    WorkDate = new DateOnly(2026, 9, 2),
                    IsWorkDay = false,
                    WorkHours = 0m
                },
                new PayPeriodWorkDay
                {
                    WorkDate = new DateOnly(2026, 9, 3),
                    IsWorkDay = true,
                    WorkHours = 4m
                }
            ]
        };

        PayrollPeriodCalendarOverlay.Apply(calendar, period);

        Assert.Equal(HrCalendarStatusConst.DayOff, calendar.Days[1].StatusCode);
        Assert.Equal(0m, calendar.Days[1].PlannedHours);
        Assert.Equal(HrCalendarStatusConst.PlannedWork, calendar.Days[2].StatusCode);
        Assert.Equal(4m, calendar.Days[2].PlannedHours);
        Assert.Equal(2m, calendar.Summary.NormWorkDays);
        Assert.Equal(12m, calendar.Summary.NormWorkHours);
    }

    [Theory]
    [InlineData("OPEN", false, true)]
    [InlineData("OPEN", true, false)]
    [InlineData("CLOSED", false, false)]
    public void EditPolicy_RequiresOpenAndUnused(string status, bool used, bool expected) =>
        Assert.Equal(expected, PayrollPeriodEditPolicy.CanEdit(status, used));

    [Fact]
    public void UsagePolicy_ActiveCancelledTimesheetStillCountsAsUsed()
    {
        var timesheet = new PayTimesheet
        {
            PeriodId = 42,
            StateId = StateIdConst.ACTIVE,
            StatusId = DocumentStatusIdConst.CANCELLED
        };

        var isUsed = PayrollPeriodEditPolicy.UsagePredicate(42).Compile()(timesheet);

        Assert.True(isUsed);
    }

    [Fact]
    public void Validate_RejectsNullWorkDatesWithoutThrowing()
    {
        var dto = new PayrollPeriodCreateDto
        {
            Year = 2026,
            Month = 9,
            DailyWorkHours = 8m,
            WorkDates = null!
        };

        var result = new PayrollPeriodCreateDtoValidator().Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(dto.WorkDates));
    }
}
