using Application.Features.Hr.Calendar;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Pay.Timesheets;

/// <summary>
/// Applies the period calendar as a template over an employee's HR calendar.
/// The HR calendar remains the source for employment and absences, while the
/// period controls which dates are payable work days and their planned hours.
/// </summary>
public static class PayrollPeriodCalendarOverlay
{
    public static HrEmployeeCalendarDto Apply(
        HrEmployeeCalendarDto calendar,
        PayPeriod period)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        ArgumentNullException.ThrowIfNull(period);

        var periodDays = period.WorkDays
            .GroupBy(day => day.WorkDate)
            .ToDictionary(group => group.Key, group => group.Last());

        foreach (var day in calendar.Days)
        {
            if (!periodDays.TryGetValue(day.Date, out var periodDay))
                continue;

            if (!periodDay.IsWorkDay || periodDay.WorkHours <= 0m)
            {
                day.StatusCode = HrCalendarStatusConst.DayOff;
                day.StatusName = "Dam olish kuni";
                day.PlannedHours = 0m;
                day.WorkedHours = 0m;
                day.AbsenceId = null;
                day.AbsenceTypeId = null;
                day.AbsenceTypeCode = null;
                day.AbsenceTypeName = null;
                day.TimesheetCategory = null;
                continue;
            }

            day.PlannedHours = decimal.Round(periodDay.WorkHours, 2);
            if (day.StatusCode == HrCalendarStatusConst.DayOff)
            {
                day.StatusCode = HrCalendarStatusConst.PlannedWork;
                day.StatusName = "Rejalashtirilgan ish kuni";
            }

            if (day.StatusCode == HrCalendarStatusConst.Worked)
                day.WorkedHours = day.PlannedHours;
        }

        RecalculateSummary(calendar);
        return calendar;
    }

    private static void RecalculateSummary(HrEmployeeCalendarDto calendar)
    {
        var summary = new HrEmployeeCalendarSummaryDto();
        foreach (var day in calendar.Days)
        {
            if (day.StatusCode == HrCalendarStatusConst.NotEmployed)
                continue;

            if (day.PlannedHours > 0m)
            {
                summary.NormWorkDays++;
                summary.NormWorkHours += day.PlannedHours;
            }

            switch (day.StatusCode)
            {
                case HrCalendarStatusConst.Worked:
                    summary.WorkedDays++;
                    summary.WorkedHours += day.WorkedHours;
                    break;
                case HrCalendarStatusConst.PlannedWork:
                    summary.PlannedWorkDays++;
                    summary.PlannedWorkHours += day.PlannedHours;
                    break;
                default:
                    switch (day.TimesheetCategory)
                    {
                        case HrTimesheetCategoryConst.Leave:
                            summary.LeaveDays++;
                            break;
                        case HrTimesheetCategoryConst.Sick:
                            summary.SickDays++;
                            break;
                        case HrTimesheetCategoryConst.Absent:
                            summary.AbsentDays++;
                            break;
                    }
                    break;
            }
        }

        summary.NormWorkHours = decimal.Round(summary.NormWorkHours, 2);
        summary.WorkedHours = decimal.Round(summary.WorkedHours, 2);
        summary.PlannedWorkHours = decimal.Round(summary.PlannedWorkHours, 2);
        calendar.Summary = summary;
    }
}
