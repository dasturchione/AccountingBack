using Application.Features.Hr.Calendar;

namespace Application.Features.Pay.Timesheets;

public sealed class PayrollTimesheetCalendarDto
{
    public long? TimesheetId { get; set; }
    public long PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public bool IsLegacy { get; set; }
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
    public string? SourceStatusCode { get; set; }
    public long? SourceAbsenceId { get; set; }
    public long? SourceScheduleId { get; set; }
    public short? SourceAbsenceTypeId { get; set; }
    public decimal PlannedHours { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal NightHours { get; set; }
    public decimal HolidayHours { get; set; }
    public decimal WeekendHours { get; set; }
    public long? ScheduleId { get; set; }
    public long? AbsenceId { get; set; }
    public short? AbsenceTypeId { get; set; }
    public string? AbsenceTypeCode { get; set; }
    public string? AbsenceTypeName { get; set; }
    public string? TimesheetCategory { get; set; }
    public bool IsOverridden { get; set; }
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
    public decimal PaidLeaveDays { get; set; }
    public decimal PaidSickDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal NightHours { get; set; }
    public decimal HolidayHours { get; set; }
    public decimal WeekendHours { get; set; }
    public string? Note { get; set; }
    public bool IsLegacy { get; set; }
    public List<PayrollTimesheetDayDto> Days { get; set; } = [];
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
        var savedDaysByEmployee = documentLines?.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Days.ToDictionary(day => day.Date));

        var result = new PayrollTimesheetCalendarDto
        {
            TimesheetId = timesheetId,
            PeriodId = periodId,
            PeriodName = periodName,
            DateFrom = dateFrom,
            DateTo = dateTo,
            IsLegacy = documentLines?.Values.Any(line => line.IsLegacy || line.Days.Count == 0) ?? false
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
                    .Where(calendar => !HasLegacyDocumentLine(documentLines, calendar.EmployeeId))
                    .Select(calendar => MapDay(
                        calendar,
                        daysByEmployee[calendar.EmployeeId][date],
                        savedDaysByEmployee is not null &&
                        savedDaysByEmployee.TryGetValue(calendar.EmployeeId, out var savedDays) &&
                        savedDays.TryGetValue(date, out var savedDay)
                            ? savedDay
                            : null))
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

    private static bool HasLegacyDocumentLine(
        IReadOnlyDictionary<long, PayrollTimesheetLineDto>? documentLines,
        long employeeId) =>
        documentLines is not null &&
        documentLines.TryGetValue(employeeId, out var line) &&
        (line.IsLegacy || line.Days.Count == 0);

    private static PayrollTimesheetCalendarEmployeeDayDto MapDay(
        HrEmployeeCalendarDto calendar,
        HrEmployeeCalendarDayDto day,
        PayrollTimesheetDayDto? savedDay)
    {
        if (savedDay is not null)
        {
            return new PayrollTimesheetCalendarEmployeeDayDto
            {
                EmployeeId = calendar.EmployeeId,
                EmployeeNumber = calendar.EmployeeNumber,
                EmployeeName = calendar.EmployeeName,
                StatusCode = savedDay.StatusCode,
                StatusName = savedDay.StatusName,
                SourceStatusCode = savedDay.SourceStatusCode,
                SourceAbsenceId = savedDay.SourceAbsenceId,
                SourceScheduleId = savedDay.SourceScheduleId,
                SourceAbsenceTypeId = savedDay.SourceAbsenceTypeId,
                PlannedHours = savedDay.PlannedHours,
                WorkedHours = savedDay.WorkedHours,
                OvertimeHours = savedDay.OvertimeHours,
                NightHours = savedDay.NightHours,
                HolidayHours = savedDay.HolidayHours,
                WeekendHours = savedDay.WeekendHours,
                ScheduleId = savedDay.SourceScheduleId,
                AbsenceId = savedDay.SourceAbsenceId,
                AbsenceTypeId = savedDay.AbsenceTypeId,
                AbsenceTypeCode = savedDay.AbsenceTypeCode,
                AbsenceTypeName = savedDay.AbsenceTypeName,
                TimesheetCategory = savedDay.TimesheetCategory,
                IsOverridden = savedDay.IsOverridden
            };
        }

        return new PayrollTimesheetCalendarEmployeeDayDto
        {
            EmployeeId = calendar.EmployeeId,
            EmployeeNumber = calendar.EmployeeNumber,
            EmployeeName = calendar.EmployeeName,
            StatusCode = day.StatusCode,
            StatusName = day.StatusName,
            SourceStatusCode = day.StatusCode,
            SourceAbsenceId = day.AbsenceId,
            SourceScheduleId = day.ScheduleId,
            SourceAbsenceTypeId = day.AbsenceTypeId,
            PlannedHours = day.PlannedHours,
            WorkedHours = day.WorkedHours,
            ScheduleId = day.ScheduleId,
            AbsenceId = day.AbsenceId,
            AbsenceTypeId = day.AbsenceTypeId,
            AbsenceTypeCode = day.AbsenceTypeCode,
            AbsenceTypeName = day.AbsenceTypeName,
            TimesheetCategory = day.TimesheetCategory
        };
    }

    private static PayrollTimesheetCalendarEmployeeSummaryDto MapSummary(
        HrEmployeeCalendarDto calendar,
        PayrollTimesheetLineDto? line)
    {
        var summary = calendar.Summary;
        var hasSavedDays = line is { Days.Count: > 0 };
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
            PlannedWorkDays = hasSavedDays
                ? line!.Days.Count(day => day.StatusCode == SharedKernel.Constants.HrCalendarStatusConst.PlannedWork)
                : summary.PlannedWorkDays,
            PlannedWorkHours = hasSavedDays
                ? line!.Days.Sum(day => day.PlannedHours)
                : summary.PlannedWorkHours,
            LeaveDays = line?.LeaveDays ?? summary.LeaveDays,
            SickDays = line?.SickDays ?? summary.SickDays,
            PaidLeaveDays = line?.PaidLeaveDays ?? 0m,
            PaidSickDays = line?.PaidSickDays ?? 0m,
            AbsentDays = line?.AbsentDays ?? summary.AbsentDays,
            OvertimeHours = line?.OvertimeHours ?? 0m,
            NightHours = line?.NightHours ?? 0m,
            HolidayHours = line?.HolidayHours ?? 0m,
            WeekendHours = line?.WeekendHours ?? 0m,
            Note = line?.Note,
            IsLegacy = line is not null && (line.IsLegacy || line.Days.Count == 0),
            Days = line?.Days ?? []
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
