using Application.Features.Hr.Calendar;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class HrEmployeeCalendarTests
{
    [Fact]
    public void Build_UsesFiveDayEightHourEmployeeScheduleAndHrAbsences()
    {
        var employment = Employment(40m);
        var schedule = Schedule((1, 8m), (2, 8m), (3, 8m), (4, 8m), (5, 8m));
        var absences = new[]
        {
            Absence(new DateOnly(2026, 1, 7), 1, "ANNUAL_LEAVE", HrTimesheetCategoryConst.Leave),
            Absence(new DateOnly(2026, 1, 8), 2, "SICK_LEAVE", HrTimesheetCategoryConst.Sick),
            Absence(new DateOnly(2026, 1, 9), 3, "UNPAID_LEAVE", HrTimesheetCategoryConst.Absent)
        };

        var result = HrEmployeeCalendarCalculator.Build(
            1,
            "EMP-1",
            "Employee One",
            new DateOnly(2026, 1, 5),
            new DateOnly(2026, 1, 11),
            new DateOnly(2026, 1, 31),
            [employment],
            [schedule],
            absences);

        Assert.Equal(5m, result.Summary.NormWorkDays);
        Assert.Equal(40m, result.Summary.NormWorkHours);
        Assert.Equal(2m, result.Summary.WorkedDays);
        Assert.Equal(16m, result.Summary.WorkedHours);
        Assert.Equal(1m, result.Summary.LeaveDays);
        Assert.Equal(1m, result.Summary.SickDays);
        Assert.Equal(1m, result.Summary.AbsentDays);
        Assert.Equal("ANNUAL_LEAVE", result.Days.Single(x => x.Date == new DateOnly(2026, 1, 7)).StatusCode);
        Assert.Equal(HrCalendarStatusConst.DayOff, result.Days.Single(x => x.Date == new DateOnly(2026, 1, 11)).StatusCode);
    }

    [Fact]
    public void Build_SupportsSixDayFourHourStudentSchedule()
    {
        var employment = Employment(24m);
        var schedule = Schedule((1, 4m), (2, 4m), (3, 4m), (4, 4m), (5, 4m), (6, 4m));

        var result = HrEmployeeCalendarCalculator.Build(
            2,
            "EMP-2",
            "Student Employee",
            new DateOnly(2026, 1, 5),
            new DateOnly(2026, 1, 11),
            new DateOnly(2026, 1, 31),
            [employment],
            [schedule],
            []);

        Assert.Equal(6m, result.Summary.NormWorkDays);
        Assert.Equal(24m, result.Summary.NormWorkHours);
        Assert.Equal(6m, result.Summary.WorkedDays);
        Assert.All(
            result.Days.Where(x => x.DayOfWeek <= 6),
            day => Assert.Equal(4m, day.PlannedHours));
    }

    [Fact]
    public void Build_UsesEmploymentWeeklyHoursAsBackwardCompatibleFallback()
    {
        var result = HrEmployeeCalendarCalculator.Build(
            3,
            "EMP-3",
            "Legacy Employee",
            new DateOnly(2026, 1, 5),
            new DateOnly(2026, 1, 11),
            new DateOnly(2026, 1, 31),
            [Employment(20m)],
            [],
            []);

        Assert.Equal(5m, result.Summary.NormWorkDays);
        Assert.Equal(20m, result.Summary.NormWorkHours);
        Assert.All(
            result.Days.Where(x => x.DayOfWeek <= 5),
            day => Assert.Equal(4m, day.PlannedHours));
    }

    [Fact]
    public void Build_DoesNotFallbackOnWeekdayMissingFromExplicitSchedule()
    {
        var result = HrEmployeeCalendarCalculator.Build(
            4,
            "EMP-4",
            "Four Day Employee",
            new DateOnly(2026, 1, 5),
            new DateOnly(2026, 1, 11),
            new DateOnly(2026, 1, 31),
            [Employment(32m)],
            [Schedule((1, 8m), (2, 8m), (3, 8m), (4, 8m))],
            []);

        var friday = result.Days.Single(x => x.Date == new DateOnly(2026, 1, 9));
        Assert.Equal(HrCalendarStatusConst.DayOff, friday.StatusCode);
        Assert.Equal(4m, result.Summary.NormWorkDays);
        Assert.Equal(32m, result.Summary.NormWorkHours);
    }

    private static PayEmployment Employment(decimal weeklyHours) =>
        new()
        {
            Id = 1,
            EmployeeId = 1,
            StateId = StateIdConst.ACTIVE,
            StartDate = new DateOnly(2025, 1, 1),
            WeeklyHours = weeklyHours,
            EmploymentType = PayrollEmploymentTypeConst.Primary
        };

    private static HrEmployeeWorkSchedule Schedule(params (short Day, decimal Hours)[] days) =>
        new()
        {
            Id = 10,
            EmployeeId = 1,
            Name = "Personal schedule",
            StateId = StateIdConst.ACTIVE,
            EffectiveFrom = new DateOnly(2025, 1, 1),
            Days = days.Select(x => new HrEmployeeWorkScheduleDay
            {
                DayOfWeek = x.Day,
                WorkHours = x.Hours
            }).ToList()
        };

    private static HrAbsence Absence(
        DateOnly date,
        short typeId,
        string code,
        string category) =>
        new()
        {
            Id = typeId,
            AbsenceTypeId = typeId,
            StartDate = date,
            EndDate = date,
            DocDate = date,
            StateId = StateIdConst.ACTIVE,
            AbsenceType = new HrAbsenceType
            {
                Id = typeId,
                Code = code,
                Name = code,
                TimesheetCategory = category,
                StateId = StateIdConst.ACTIVE
            }
        };
}
