using Application.Features.Hr.Calendar;
using Domain.Entities;

namespace Application.Features.Pay.Timesheets;

public readonly record struct PayrollTimesheetNorm(decimal NormWorkDays, decimal NormWorkHours);

public static class PayrollTimesheetNormCalculator
{
    public static PayrollTimesheetNorm Resolve(
        PayPeriod period,
        HrEmployeeCalendarDto? employeeCalendar,
        decimal? snapshotNormWorkDays = null,
        decimal? snapshotNormWorkHours = null,
        IReadOnlyCollection<DateOnly>? periodWorkDates = null)
    {
        if (snapshotNormWorkDays is > 0m && snapshotNormWorkHours is > 0m)
            return new PayrollTimesheetNorm(snapshotNormWorkDays.Value, snapshotNormWorkHours.Value);

        var summary = employeeCalendar?.Summary;
        if (employeeCalendar is not null && periodWorkDates is { Count: > 0 })
        {
            var selectedDates = periodWorkDates.ToHashSet();
            var scheduledDays = employeeCalendar.Days
                .Where(day => selectedDates.Contains(day.Date) && day.PlannedHours > 0m)
                .ToList();
            return new PayrollTimesheetNorm(
                scheduledDays.Count,
                decimal.Round(scheduledDays.Sum(day => day.PlannedHours), 2));
        }

        if (summary is not null && summary.NormWorkDays > 0m && summary.NormWorkHours > 0m)
            return new PayrollTimesheetNorm(summary.NormWorkDays, summary.NormWorkHours);

        return new PayrollTimesheetNorm(period.NormWorkDays, period.NormWorkHours);
    }
}
