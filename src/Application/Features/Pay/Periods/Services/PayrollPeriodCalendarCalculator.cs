using Domain.Entities;
using SharedKernel.Constants;
using System.Linq.Expressions;

namespace Application.Features.Pay.Periods;

public sealed record PayrollPeriodCalendarResult(
    IReadOnlyList<DateOnly> WorkDates,
    decimal NormWorkDays,
    decimal NormWorkHours,
    IReadOnlyList<PayrollPeriodCalendarDaySaveDto> CalendarDays);

public static class PayrollPeriodCalendarCalculator
{
    public static PayrollPeriodCalendarResult Calculate(
        short year,
        short month,
        decimal dailyWorkHours,
        IEnumerable<DateOnly> workDates)
    {
        var normalizedDates = workDates
            .Distinct()
            .Order()
            .ToArray();
        var normWorkDays = (decimal)normalizedDates.Length;

        return new PayrollPeriodCalendarResult(
            normalizedDates,
            normWorkDays,
            decimal.Round(normWorkDays * dailyWorkHours, 2),
            normalizedDates.Select(date => new PayrollPeriodCalendarDaySaveDto
            {
                Date = date,
                DayType = PayrollPeriodDayTypeConst.Normal,
                WorkHours = dailyWorkHours
            }).ToArray());
    }

    public static PayrollPeriodCalendarResult CalculateFromCalendarDays(
        short year,
        short month,
        decimal dailyWorkHours,
        IEnumerable<PayrollPeriodCalendarDaySaveDto> calendarDays)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var normalized = calendarDays
            .GroupBy(day => day.Date)
            .Select(group => group.Last())
            .Where(day => day.Date >= start && day.Date <= end)
            .OrderBy(day => day.Date)
            .Select(day => new PayrollPeriodCalendarDaySaveDto
            {
                Date = day.Date,
                DayType = PayrollPeriodDayTypeConst.All.Contains(day.DayType)
                    ? day.DayType
                    : PayrollPeriodDayTypeConst.Normal,
                WorkHours = day.DayType == PayrollPeriodDayTypeConst.Holiday
                    ? 0m
                    : decimal.Clamp(day.WorkHours > 0m ? day.WorkHours : dailyWorkHours, 0m, 24m)
            })
            .ToArray();
        var workDates = normalized
            .Where(day => day.DayType != PayrollPeriodDayTypeConst.Holiday && day.WorkHours > 0m)
            .Select(day => day.Date)
            .ToArray();

        return new PayrollPeriodCalendarResult(
            workDates,
            workDates.Length,
            decimal.Round(normalized.Where(day => workDates.Contains(day.Date)).Sum(day => day.WorkHours), 2),
            normalized);
    }
}

public static class PayrollPeriodEditPolicy
{
    public const string ClosedReason = "Period is closed.";
    public const string UsedInTimesheetReason = "Period is already used in a timesheet.";

    public static bool CanEdit(string status, bool isUsedInTimesheet) =>
        status == PayrollPeriodStatusConst.Open && !isUsedInTimesheet;

    public static Expression<Func<PayTimesheet, bool>> UsagePredicate(long periodId) =>
        timesheet => timesheet.PeriodId == periodId && timesheet.StateId == StateIdConst.ACTIVE;

    public static string? GetBlockedReason(string status, bool isUsedInTimesheet) =>
        status != PayrollPeriodStatusConst.Open
            ? ClosedReason
            : isUsedInTimesheet
                ? UsedInTimesheetReason
                : null;
}
