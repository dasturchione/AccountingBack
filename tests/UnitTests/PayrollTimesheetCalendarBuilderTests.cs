using Application.Features.Hr.Calendar;
using Application.Features.Pay.Timesheets;

namespace UnitTests;

public sealed class PayrollTimesheetCalendarBuilderTests
{
    [Fact]
    public void Build_ReturnsDatesWithAllEmployeesAndDocumentMonthlySummary()
    {
        var dateFrom = new DateOnly(2026, 8, 3);
        var dateTo = new DateOnly(2026, 8, 4);
        var first = Calendar(
            1,
            "001",
            "Alpha Employee",
            dateFrom,
            dateTo,
            Day(dateFrom, "WORKED", 8m, 8m),
            Day(dateTo, "SICK_LEAVE", 8m, 0m, absenceId: 15));
        first.Summary = new HrEmployeeCalendarSummaryDto
        {
            NormWorkDays = 2m,
            NormWorkHours = 16m,
            WorkedDays = 1m,
            WorkedHours = 8m,
            SickDays = 1m
        };
        var second = Calendar(
            2,
            "002",
            "Beta Employee",
            dateFrom,
            dateTo,
            Day(dateFrom, "WORKED", 4m, 4m),
            Day(dateTo, "WORKED", 4m, 4m));
        second.Summary = new HrEmployeeCalendarSummaryDto
        {
            NormWorkDays = 2m,
            NormWorkHours = 8m,
            WorkedDays = 2m,
            WorkedHours = 8m
        };
        var documentLine = new PayrollTimesheetLineDto
        {
            Id = 100,
            EmployeeId = 1,
            EmployeeNumber = "001",
            EmployeeName = "Alpha Employee",
            NormWorkDays = 2m,
            NormWorkHours = 16m,
            WorkedDays = 1m,
            WorkedHours = 7.5m,
            SickDays = 1m,
            OvertimeHours = 0.5m,
            Note = "Adjusted"
        };

        var result = PayrollTimesheetCalendarBuilder.Build(
            10,
            20,
            "2026-8",
            dateFrom,
            dateTo,
            [second, first],
            new Dictionary<long, PayrollTimesheetLineDto> { [1] = documentLine });

        Assert.Equal(2, result.DailyAttendance.Count);
        Assert.All(result.DailyAttendance, day => Assert.Equal(2, day.Employees.Count));
        Assert.Equal("Alpha Employee", result.DailyAttendance[0].Employees[0].EmployeeName);
        Assert.Equal("SICK_LEAVE", result.DailyAttendance[1].Employees[0].StatusCode);
        Assert.Equal(15, result.DailyAttendance[1].Employees[0].AbsenceId);

        var firstSummary = result.MonthlySummary[0];
        Assert.True(firstSummary.IsIncludedInDocument);
        Assert.Equal(100, firstSummary.TimesheetLineId);
        Assert.Equal(7.5m, firstSummary.WorkedHours);
        Assert.Equal(0.5m, firstSummary.OvertimeHours);
        Assert.Equal("Adjusted", firstSummary.Note);

        var secondSummary = result.MonthlySummary[1];
        Assert.False(secondSummary.IsIncludedInDocument);
        Assert.Equal(8m, secondSummary.WorkedHours);
    }

    private static HrEmployeeCalendarDto Calendar(
        long employeeId,
        string employeeNumber,
        string employeeName,
        DateOnly dateFrom,
        DateOnly dateTo,
        params HrEmployeeCalendarDayDto[] days) =>
        new()
        {
            EmployeeId = employeeId,
            EmployeeNumber = employeeNumber,
            EmployeeName = employeeName,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Days = days.ToList()
        };

    private static HrEmployeeCalendarDayDto Day(
        DateOnly date,
        string statusCode,
        decimal plannedHours,
        decimal workedHours,
        long? absenceId = null) =>
        new()
        {
            Date = date,
            DayOfWeek = (short)(((int)date.DayOfWeek + 6) % 7 + 1),
            StatusCode = statusCode,
            StatusName = statusCode,
            PlannedHours = plannedHours,
            WorkedHours = workedHours,
            AbsenceId = absenceId
        };
}
