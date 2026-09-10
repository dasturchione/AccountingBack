using SharedKernel.Constants;

namespace Application.Features.Pay.Timesheets;

/// <summary>Category and status values used by the attendance status picker.</summary>
public static class PayrollAttendanceStatusKindConst
{
    public const string Fixed = "FIXED";
    public const string Absence = "ABSENCE";
}

/// <summary>A status option exposed to the timesheet editor.</summary>
public sealed class PayrollAttendanceStatusOptionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Kind { get; set; } = null!;
    public short? AbsenceTypeId { get; set; }
    public string? TimesheetCategory { get; set; }
    public bool? IsPaid { get; set; }
}

/// <summary>Fixed statuses are always available; absence options are supplied by HR.</summary>
public static class PayrollAttendanceStatusOptions
{
    private static readonly string[] FixedCodes =
    [
        HrCalendarStatusConst.Worked,
        HrCalendarStatusConst.PlannedWork,
        HrCalendarStatusConst.DayOff,
        HrCalendarStatusConst.NotEmployed
    ];

    /// <summary>Returns a defensive copy of the fixed option templates.</summary>
    public static IReadOnlyList<PayrollAttendanceStatusOptionDto> Fixed => GetFixed();

    /// <summary>Returns fresh mutable DTOs so consumers cannot alter shared option templates.</summary>
    public static IReadOnlyList<PayrollAttendanceStatusOptionDto> GetFixed(short? languageId = null) =>
        FixedCodes.Select(code => new PayrollAttendanceStatusOptionDto
        {
            Code = code,
            Name = GetName(code, languageId ?? LanguageIdConst.UZ),
            Kind = PayrollAttendanceStatusKindConst.Fixed
        }).ToList();

    public static bool TryGetFixed(
        string statusCode,
        out PayrollAttendanceStatusOptionDto option,
        short? languageId = null)
    {
        var code = FixedCodes.FirstOrDefault(x =>
            string.Equals(x, statusCode, StringComparison.OrdinalIgnoreCase));
        if (code is null)
        {
            option = null!;
            return false;
        }

        option = new PayrollAttendanceStatusOptionDto
        {
            Code = code,
            Name = GetName(code, languageId ?? LanguageIdConst.UZ),
            Kind = PayrollAttendanceStatusKindConst.Fixed
        };
        return true;
    }

    private static string GetName(string code, short? languageId) =>
        (code, languageId) switch
        {
            (HrCalendarStatusConst.Worked, LanguageIdConst.UZ) => "Ishlagan",
            (HrCalendarStatusConst.PlannedWork, LanguageIdConst.UZ) => "Rejalashtirilgan ish kuni",
            (HrCalendarStatusConst.DayOff, LanguageIdConst.UZ) => "Dam olish kuni",
            (HrCalendarStatusConst.NotEmployed, LanguageIdConst.UZ) => "Ishga qabul qilinmagan",
            (HrCalendarStatusConst.Worked, LanguageIdConst.UZ_CYRL) => "Ишлаган",
            (HrCalendarStatusConst.PlannedWork, LanguageIdConst.UZ_CYRL) => "Режалаштирилган иш куни",
            (HrCalendarStatusConst.DayOff, LanguageIdConst.UZ_CYRL) => "Дам олиш куни",
            (HrCalendarStatusConst.NotEmployed, LanguageIdConst.UZ_CYRL) => "Ишга қабул қилинмаган",
            (HrCalendarStatusConst.Worked, LanguageIdConst.RU) => "Отработан",
            (HrCalendarStatusConst.PlannedWork, LanguageIdConst.RU) => "Запланированный рабочий день",
            (HrCalendarStatusConst.DayOff, LanguageIdConst.RU) => "Выходной день",
            (HrCalendarStatusConst.NotEmployed, LanguageIdConst.RU) => "Не трудоустроен",
            (HrCalendarStatusConst.Worked, _) => "Worked",
            (HrCalendarStatusConst.PlannedWork, _) => "Planned work day",
            (HrCalendarStatusConst.DayOff, _) => "Day off",
            (HrCalendarStatusConst.NotEmployed, _) => "Not employed",
            _ => code
        };
}

/// <summary>Resolves a submitted status against fixed and active HR options.</summary>
public static class PayrollAttendanceStatusResolver
{
    public static PayrollAttendanceStatusOptionDto Resolve(
        string statusCode,
        short? absenceTypeId,
        IReadOnlyCollection<PayrollAttendanceStatusOptionDto> options)
    {
        if (string.IsNullOrWhiteSpace(statusCode))
            throw new ArgumentException("Attendance status code is required.", nameof(statusCode));

        if (PayrollAttendanceStatusOptions.TryGetFixed(statusCode, out var fixedOption))
        {
            if (absenceTypeId.HasValue)
                throw new ArgumentException("Fixed attendance statuses cannot specify an absence type.", nameof(absenceTypeId));

            return fixedOption;
        }

        if (!absenceTypeId.HasValue)
            throw new ArgumentException("An absence status requires an absence type.", nameof(absenceTypeId));

        var absenceOption = options.FirstOrDefault(x =>
            x.Kind == PayrollAttendanceStatusKindConst.Absence &&
            x.AbsenceTypeId == absenceTypeId &&
            string.Equals(x.Code, statusCode, StringComparison.OrdinalIgnoreCase));
        if (absenceOption is null)
            throw new ArgumentException("The attendance status and absence type do not match.", nameof(statusCode));
        if (!HrTimesheetCategoryConst.All.Contains(absenceOption.TimesheetCategory))
            throw new ArgumentException("The absence type has an invalid timesheet category.", nameof(options));

        return absenceOption;
    }
}

public sealed record PayrollTimesheetDayValue(
    string StatusCode,
    string? TimesheetCategory,
    decimal? WorkedHours = null,
    bool? IsPaid = null,
    decimal? PlannedHours = null,
    decimal OvertimeHours = 0m,
    decimal NightHours = 0m,
    decimal HolidayHours = 0m,
    decimal WeekendHours = 0m);

public sealed record PayrollTimesheetDayTotals(
    decimal WorkedDays,
    decimal WorkedHours,
    decimal PlannedWorkDays,
    decimal PlannedWorkHours,
    decimal LeaveDays,
    decimal SickDays,
    decimal AbsentDays,
    decimal PaidLeaveDays = 0m,
    decimal PaidSickDays = 0m,
    decimal OvertimeHours = 0m,
    decimal NightHours = 0m,
    decimal HolidayHours = 0m,
    decimal WeekendHours = 0m);

/// <summary>Validates that submitted day snapshots cover a period exactly once.</summary>
public static class PayrollTimesheetDayCoverage
{
    public static IReadOnlySet<DateOnly> CreateExpected(DateOnly dateFrom, DateOnly dateTo)
    {
        if (dateTo < dateFrom)
            throw new ArgumentException("The period end date cannot precede the start date.", nameof(dateTo));

        var dates = new HashSet<DateOnly>();
        for (var date = dateFrom; date <= dateTo; date = date.AddDays(1))
            dates.Add(date);

        return dates;
    }

    public static bool IsComplete(IReadOnlySet<DateOnly> expectedDates, IEnumerable<DateOnly> submittedDates)
    {
        ArgumentNullException.ThrowIfNull(expectedDates);
        ArgumentNullException.ThrowIfNull(submittedDates);

        var submitted = submittedDates.ToList();
        return submitted.Count == expectedDates.Count &&
               submitted.Distinct().Count() == submitted.Count &&
               submitted.All(expectedDates.Contains);
    }
}

/// <summary>Calculates line aggregates from daily statuses and the period's fixed daily hours.</summary>
public static class PayrollTimesheetDayCalculator
{
    public static PayrollTimesheetDayTotals Calculate(
        decimal dailyWorkHours,
        IEnumerable<PayrollTimesheetDayValue> days)
    {
        ArgumentNullException.ThrowIfNull(days);
        if (dailyWorkHours < 0m)
            throw new ArgumentOutOfRangeException(nameof(dailyWorkHours), "Daily work hours cannot be negative.");

        var values = days.ToList();
        var workedDays = values.Count(x => x.StatusCode == HrCalendarStatusConst.Worked);
        var plannedWorkDays = values.Count(x => x.StatusCode == HrCalendarStatusConst.PlannedWork);
        var leaveDays = values.Count(x => x.TimesheetCategory == HrTimesheetCategoryConst.Leave);
        var sickDays = values.Count(x => x.TimesheetCategory == HrTimesheetCategoryConst.Sick);
        var absentDays = values.Count(x => x.TimesheetCategory == HrTimesheetCategoryConst.Absent);
        var paidLeaveDays = values.Count(x =>
            x.TimesheetCategory == HrTimesheetCategoryConst.Leave && x.IsPaid == true);
        var paidSickDays = values.Count(x =>
            x.TimesheetCategory == HrTimesheetCategoryConst.Sick && x.IsPaid == true);

        var plannedWorkHours = values
            .Where(x => x.StatusCode == HrCalendarStatusConst.PlannedWork)
            .Sum(x => x.PlannedHours ?? dailyWorkHours);
        var workedHours = values
            .Where(x => x.StatusCode == HrCalendarStatusConst.Worked)
            .Sum(x => x.WorkedHours ?? dailyWorkHours);
        var overtimeHours = values.Sum(x => x.OvertimeHours);
        var nightHours = values.Sum(x => x.NightHours);
        var holidayHours = values.Sum(x => x.HolidayHours);
        var weekendHours = values.Sum(x => x.WeekendHours);

        return new PayrollTimesheetDayTotals(
            workedDays,
            decimal.Round(workedHours, 2),
            plannedWorkDays,
            decimal.Round(plannedWorkHours, 2),
            leaveDays,
            sickDays,
            absentDays,
            paidLeaveDays,
            paidSickDays,
            decimal.Round(overtimeHours, 2),
            decimal.Round(nightHours, 2),
            decimal.Round(holidayHours, 2),
            decimal.Round(weekendHours, 2));
    }
}
