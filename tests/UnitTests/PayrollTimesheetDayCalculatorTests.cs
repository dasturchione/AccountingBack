using Application.Features.Pay.Timesheets;
using Application.Features.Pay.Validators;
using Application.Features.Hr.Calendar;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollTimesheetDayCalculatorTests
{
    [Fact]
    public void PaidAbsenceIsTrackedSeparatelyFromWorkedDays()
    {
        var totals = PayrollTimesheetDayCalculator.Calculate(
            8m,
            [
                new PayrollTimesheetDayValue(HrCalendarStatusConst.Worked, null, 8m),
                new PayrollTimesheetDayValue("ANNUAL_LEAVE", HrTimesheetCategoryConst.Leave, 0m, true),
                new PayrollTimesheetDayValue("UNPAID_LEAVE", HrTimesheetCategoryConst.Leave, 0m, false)
            ]);

        Assert.Equal(1m, totals.WorkedDays);
        Assert.Equal(1m, totals.PaidLeaveDays);
        Assert.Equal(2m, totals.LeaveDays);
    }

    [Fact]
    public void PlannedWorkUsesTheDaySnapshotHours()
    {
        var totals = PayrollTimesheetDayCalculator.Calculate(
            8m,
            [new PayrollTimesheetDayValue(
                HrCalendarStatusConst.PlannedWork,
                null,
                0m,
                null,
                4m)]);

        Assert.Equal(1m, totals.PlannedWorkDays);
        Assert.Equal(4m, totals.PlannedWorkHours);
    }

    [Fact]
    public void SpecialHoursAreAggregatedFromDailySnapshots()
    {
        var totals = PayrollTimesheetDayCalculator.Calculate(
            8m,
            [new PayrollTimesheetDayValue(
                HrCalendarStatusConst.Worked,
                null,
                8m,
                null,
                null,
            OvertimeHours: 2m,
            NightHours: 3m,
            HolidayHours: 1m,
            WeekendHours: 0m)]);

        Assert.Equal(2m, totals.OvertimeHours);
        Assert.Equal(3m, totals.NightHours);
        Assert.Equal(1m, totals.HolidayHours);
    }
    [Fact]
    public void TimesheetListDto_ContainsWorkedTotalsForAdminList()
    {
        var dto = new PayrollTimesheetListDto
        {
            TotalWorkedDays = 4m,
            TotalWorkedHours = 32m
        };

        Assert.Equal(4m, dto.TotalWorkedDays);
        Assert.Equal(32m, dto.TotalWorkedHours);
    }

    [Fact]
    public void Coverage_RequiresEveryPeriodDateExactlyOnce()
    {
        var expected = PayrollTimesheetDayCoverage.CreateExpected(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 3));

        Assert.True(PayrollTimesheetDayCoverage.IsComplete(
            expected,
            [new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 2), new DateOnly(2026, 9, 3)]));
        Assert.False(PayrollTimesheetDayCoverage.IsComplete(
            expected,
            [new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)]));
        Assert.False(PayrollTimesheetDayCoverage.IsComplete(
            expected,
            [new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 2),
             new DateOnly(2026, 9, 2), new DateOnly(2026, 9, 3)]));
    }

    [Fact]
    public void FixedOptions_DoNotShareMutableInstances()
    {
        var first = PayrollAttendanceStatusOptions.Fixed;
        first[0].Name = "mutated";

        var second = PayrollAttendanceStatusOptions.Fixed;

        Assert.NotEqual("mutated", second[0].Name);
    }

    [Fact]
    public void CalendarBuilder_UsesSavedDaySnapshotBeforeRecalculatedHrDay()
    {
        var date = new DateOnly(2026, 9, 1);
        var hrCalendar = new HrEmployeeCalendarDto
        {
            EmployeeId = 10,
            EmployeeNumber = "E-10",
            EmployeeName = "Employee",
            Days =
            [
                new()
                {
                    Date = date,
                    StatusCode = HrCalendarStatusConst.Worked,
                    StatusName = "Worked from HR",
                    WorkedHours = 8m,
                    PlannedHours = 8m
                }
            ]
        };
        var line = new PayrollTimesheetLineDto
        {
            Id = 99,
            EmployeeId = 10,
            EmployeeNumber = "E-10",
            EmployeeName = "Employee",
            Days =
            [
                new()
                {
                    Date = date,
                    SourceStatusCode = HrCalendarStatusConst.Worked,
                    StatusCode = HrCalendarStatusConst.PlannedWork,
                    StatusName = "Saved planned work",
                    PlannedHours = 8m,
                    WorkedHours = 0m,
                    IsOverridden = true
                }
            ]
        };

        var calendar = PayrollTimesheetCalendarBuilder.Build(
            1,
            2,
            "2026-9",
            date,
            date,
            [hrCalendar],
            new Dictionary<long, PayrollTimesheetLineDto> { [10] = line });

        var day = Assert.Single(Assert.Single(calendar.DailyAttendance).Employees);
        Assert.Equal(HrCalendarStatusConst.PlannedWork, day.StatusCode);
        Assert.Equal("Saved planned work", day.StatusName);
        Assert.Equal(8m, day.PlannedHours);
        Assert.Equal(0m, day.WorkedHours);
        Assert.True(day.IsOverridden);

        var summary = Assert.Single(calendar.MonthlySummary);
        Assert.Equal(1m, summary.PlannedWorkDays);
        Assert.Equal(8m, summary.PlannedWorkHours);
    }

    [Fact]
    public void CalendarBuilder_DoesNotFabricateDailyHistoryForLegacyLine()
    {
        var date = new DateOnly(2026, 9, 1);
        var hrCalendar = new HrEmployeeCalendarDto
        {
            EmployeeId = 10,
            EmployeeNumber = "E-10",
            EmployeeName = "Employee",
            Days =
            [
                new()
                {
                    Date = date,
                    StatusCode = HrCalendarStatusConst.Worked,
                    StatusName = "Current HR value",
                    WorkedHours = 8m
                }
            ]
        };
        var legacyLine = new PayrollTimesheetLineDto
        {
            Id = 99,
            EmployeeId = 10,
            EmployeeNumber = "E-10",
            EmployeeName = "Employee",
            IsLegacy = true,
            WorkedDays = 7m,
            WorkedHours = 56m
        };

        var calendar = PayrollTimesheetCalendarBuilder.Build(
            1,
            2,
            "2026-9",
            date,
            date,
            [hrCalendar],
            new Dictionary<long, PayrollTimesheetLineDto> { [10] = legacyLine });

        Assert.True(calendar.IsLegacy);
        Assert.Empty(Assert.Single(calendar.DailyAttendance).Employees);

        var summary = Assert.Single(calendar.MonthlySummary);
        Assert.True(summary.IsLegacy);
        Assert.Empty(summary.Days);
        Assert.Equal(7m, summary.WorkedDays);
        Assert.Equal(56m, summary.WorkedHours);
    }

    [Fact]
    public void Calculate_UsesFixedHoursAndAbsenceCategories()
    {
        var totals = PayrollTimesheetDayCalculator.Calculate(8m,
        [
            new("WORKED", null), new("WORKED", null), new("PLANNED_WORK", null),
            new("SICK_LEAVE", "SICK"), new("ANNUAL_LEAVE", "LEAVE"),
            new("UNEXCUSED_ABSENCE", "ABSENT"), new("DAY_OFF", null)
        ]);

        Assert.Equal((2m, 16m, 1m, 8m, 1m, 1m, 1m),
            (totals.WorkedDays, totals.WorkedHours, totals.PlannedWorkDays,
             totals.PlannedWorkHours, totals.LeaveDays, totals.SickDays, totals.AbsentDays));
    }

    [Fact]
    public void Calculate_UsesEnteredWorkedHoursAndDefaultsMissingHours()
    {
        var totals = PayrollTimesheetDayCalculator.Calculate(8m,
        [
            new("WORKED", null, 4m),
            new("WORKED", null, 3m),
            new("WORKED", null, null)
        ]);

        Assert.Equal(3m, totals.WorkedDays);
        Assert.Equal(15m, totals.WorkedHours);
    }

    [Fact]
    public void Resolver_RejectsAbsenceIdForWorked() =>
        Assert.Throws<ArgumentException>(() =>
            PayrollAttendanceStatusResolver.Resolve(HrCalendarStatusConst.Worked, 2, []));

    [Fact]
    public void Resolver_RequiresMatchingAbsenceTypeForAbsenceStatus()
    {
        var options = new[]
        {
            new PayrollAttendanceStatusOptionDto
            {
                Code = "SICK_LEAVE",
                Name = "Sick leave",
                Kind = PayrollAttendanceStatusKindConst.Absence,
                AbsenceTypeId = 2,
                TimesheetCategory = HrTimesheetCategoryConst.Sick
            }
        };

        var resolved = PayrollAttendanceStatusResolver.Resolve("SICK_LEAVE", 2, options);

        Assert.Equal("SICK", resolved.TimesheetCategory);
        Assert.Throws<ArgumentException>(() =>
            PayrollAttendanceStatusResolver.Resolve("SICK_LEAVE", 3, options));
    }

    [Fact]
    public void LineValidator_RequiresUniqueDaysAndValidStatus()
    {
        var dto = new PayrollTimesheetLineSaveDto
        {
            EmployeeId = 10,
            Days =
            [
                new() { Date = new DateOnly(2026, 9, 1), StatusCode = "WORKED" },
                new() { Date = new DateOnly(2026, 9, 1), StatusCode = new string('x', 51) }
            ]
        };

        var result = new PayrollTimesheetLineSaveDtoValidator().Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.PropertyName.Contains("StatusCode", StringComparison.Ordinal));
    }

    [Fact]
    public void LineValidator_RejectsNegativeOvertime()
    {
        var dto = new PayrollTimesheetLineSaveDto
        {
            EmployeeId = 10,
            OvertimeHours = -1m,
            Days = [new() { Date = new DateOnly(2026, 9, 1), StatusCode = "WORKED" }]
        };

        var result = new PayrollTimesheetLineSaveDtoValidator().Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(dto.OvertimeHours));
    }
}
