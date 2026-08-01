using Application.Features.Hr.Calendar;

namespace Application.Features.Pay.Timesheets;

public sealed class PayrollTimesheetCalendarDto
{
    public long? TimesheetId { get; set; }
    public long PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public List<PayrollTimesheetCalendarDateDto> DailyAttendance { get; set; } = [];
    public List<PayrollTimesheetCalendarEmployeeSummaryDto> MonthlySummary { get; set; } = [];
}

public sealed class PayrollTimesheetCalendarDateDto
{
    public DateOnly Date { get; set; }
    public short DayOfWeek { get; set; }
    public string DayName { get; set; } = null!;
    public List<PayrollTimesheetCalendarEmployeeDayDto> Employees { get; set; } = [];
}

public sealed class PayrollTimesheetCalendarEmployeeDayDto
{
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public decimal PlannedHours { get; set; }
    public decimal WorkedHours { get; set; }
    public long? ScheduleId { get; set; }
    public long? AbsenceId { get; set; }
    public short? AbsenceTypeId { get; set; }
    public string? AbsenceTypeCode { get; set; }
    public string? AbsenceTypeName { get; set; }
    public string? TimesheetCategory { get; set; }
}

public sealed class PayrollTimesheetCalendarEmployeeSummaryDto
{
    public long? TimesheetLineId { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public bool IsIncludedInDocument { get; set; }
    public decimal NormWorkDays { get; set; }
    public decimal NormWorkHours { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal PlannedWorkDays { get; set; }
    public decimal PlannedWorkHours { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal SickDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public string? Note { get; set; }
}

public static class PayrollTimesheetCalendarBuilder
{
    public static PayrollTimesheetCalendarDto Build(
        long? timesheetId,
        long periodId,
        string periodName,
        DateOnly dateFrom,
        DateOnly dateTo,
        IReadOnlyCollection<HrEmployeeCalendarDto> employeeCalendars,
        IReadOnlyDictionary<long, PayrollTimesheetLineDto>? documentLines = null)
    {
        var calendars = employeeCalendars
            .OrderBy(x => x.EmployeeName)
            .ThenBy(x => x.EmployeeNumber)
            .ToList();
        var daysByEmployee = calendars.ToDictionary(
            x => x.EmployeeId,
            x => x.Days.ToDictionary(day => day.Date));

        var result = new PayrollTimesheetCalendarDto
        {
            TimesheetId = timesheetId,
            PeriodId = periodId,
            PeriodName = periodName,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        for (var date = dateFrom; date <= dateTo; date = date.AddDays(1))
        {
            var dayOfWeek = ToIsoDayOfWeek(date);
            result.DailyAttendance.Add(new PayrollTimesheetCalendarDateDto
            {
                Date = date,
                DayOfWeek = dayOfWeek,
                DayName = GetDayName(dayOfWeek),
                Employees = calendars
                    .Select(calendar => MapDay(calendar, daysByEmployee[calendar.EmployeeId][date]))
                    .ToList()
            });
        }

        result.MonthlySummary = calendars
            .Select(calendar => MapSummary(
                calendar,
                documentLines is not null && documentLines.TryGetValue(calendar.EmployeeId, out var line)
                    ? line
                    : null))
            .ToList();

        return result;
    }

    private static PayrollTimesheetCalendarEmployeeDayDto MapDay(
        HrEmployeeCalendarDto calendar,
        HrEmployeeCalendarDayDto day) =>
        new()
        {
            EmployeeId = calendar.EmployeeId,
            EmployeeNumber = calendar.EmployeeNumber,
            EmployeeName = calendar.EmployeeName,
            StatusCode = day.StatusCode,
            StatusName = day.StatusName,
            PlannedHours = day.PlannedHours,
            WorkedHours = day.WorkedHours,
            ScheduleId = day.ScheduleId,
            AbsenceId = day.AbsenceId,
            AbsenceTypeId = day.AbsenceTypeId,
            AbsenceTypeCode = day.AbsenceTypeCode,
            AbsenceTypeName = day.AbsenceTypeName,
            TimesheetCategory = day.TimesheetCategory
        };

    private static PayrollTimesheetCalendarEmployeeSummaryDto MapSummary(
        HrEmployeeCalendarDto calendar,
        PayrollTimesheetLineDto? line)
    {
        var summary = calendar.Summary;
        return new PayrollTimesheetCalendarEmployeeSummaryDto
        {
            TimesheetLineId = line?.Id,
            EmployeeId = calendar.EmployeeId,
            EmployeeNumber = calendar.EmployeeNumber,
            EmployeeName = calendar.EmployeeName,
            IsIncludedInDocument = line is not null,
            NormWorkDays = line?.NormWorkDays ?? summary.NormWorkDays,
            NormWorkHours = line?.NormWorkHours ?? summary.NormWorkHours,
            WorkedDays = line?.WorkedDays ?? summary.WorkedDays,
            WorkedHours = line?.WorkedHours ?? summary.WorkedHours,
            PlannedWorkDays = summary.PlannedWorkDays,
            PlannedWorkHours = summary.PlannedWorkHours,
            LeaveDays = line?.LeaveDays ?? summary.LeaveDays,
            SickDays = line?.SickDays ?? summary.SickDays,
            AbsentDays = line?.AbsentDays ?? summary.AbsentDays,
            OvertimeHours = line?.OvertimeHours ?? 0m,
            Note = line?.Note
        };
    }

    private static short ToIsoDayOfWeek(DateOnly date) =>
        (short)(((int)date.DayOfWeek + 6) % 7 + 1);

    private static string GetDayName(short dayOfWeek) =>
        dayOfWeek switch
        {
            1 => "Dushanba",
            2 => "Seshanba",
            3 => "Chorshanba",
            4 => "Payshanba",
            5 => "Juma",
            6 => "Shanba",
            7 => "Yakshanba",
            _ => string.Empty
        };
}
