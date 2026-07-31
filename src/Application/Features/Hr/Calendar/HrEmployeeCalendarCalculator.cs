using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Hr.Calendar;

public static class HrEmployeeCalendarCalculator
{
    public static HrEmployeeCalendarDto Build(
        long employeeId,
        string employeeNumber,
        string employeeName,
        DateOnly dateFrom,
        DateOnly dateTo,
        DateOnly today,
        IReadOnlyCollection<PayEmployment> employments,
        IReadOnlyCollection<HrEmployeeWorkSchedule> schedules,
        IReadOnlyCollection<HrAbsence> absences)
    {
        var result = new HrEmployeeCalendarDto
        {
            EmployeeId = employeeId,
            EmployeeNumber = employeeNumber,
            EmployeeName = employeeName,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        for (var date = dateFrom; date <= dateTo; date = date.AddDays(1))
        {
            var employment = employments
                .Where(x => x.StateId == StateIdConst.ACTIVE &&
                            x.StartDate <= date &&
                            (!x.EndDate.HasValue || x.EndDate.Value >= date))
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefault();

            var dayOfWeek = ToIsoDayOfWeek(date);
            var day = new HrEmployeeCalendarDayDto
            {
                Date = date,
                DayOfWeek = dayOfWeek,
                StatusCode = HrCalendarStatusConst.NotEmployed,
                StatusName = "Not employed"
            };

            if (employment is null)
            {
                result.Days.Add(day);
                continue;
            }

            var schedule = schedules
                .Where(x => x.StateId == StateIdConst.ACTIVE &&
                            x.EffectiveFrom <= date &&
                            (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= date))
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefault();

            var scheduleDay = schedule?.Days.FirstOrDefault(x => x.DayOfWeek == dayOfWeek);
            var plannedHours = schedule is null
                ? GetFallbackHours(employment, dayOfWeek)
                : scheduleDay?.WorkHours ?? 0m;
            day.ScheduleId = schedule?.Id;
            day.PlannedHours = plannedHours;

            if (plannedHours <= 0m)
            {
                day.StatusCode = HrCalendarStatusConst.DayOff;
                day.StatusName = "Day off";
                result.Days.Add(day);
                continue;
            }

            result.Summary.NormWorkDays++;
            result.Summary.NormWorkHours += plannedHours;

            var absence = absences
                .Where(x => x.StateId == StateIdConst.ACTIVE &&
                            x.StartDate <= date &&
                            x.EndDate >= date)
                .OrderByDescending(x => x.DocDate)
                .FirstOrDefault();

            if (absence is not null)
            {
                day.StatusCode = absence.AbsenceType.Code;
                day.StatusName = absence.AbsenceType.Name;
                day.AbsenceId = absence.Id;
                day.AbsenceTypeId = absence.AbsenceTypeId;
                day.AbsenceTypeCode = absence.AbsenceType.Code;
                day.AbsenceTypeName = absence.AbsenceType.Name;
                day.TimesheetCategory = absence.AbsenceType.TimesheetCategory;

                switch (absence.AbsenceType.TimesheetCategory)
                {
                    case HrTimesheetCategoryConst.Leave:
                        result.Summary.LeaveDays++;
                        break;
                    case HrTimesheetCategoryConst.Sick:
                        result.Summary.SickDays++;
                        break;
                    default:
                        result.Summary.AbsentDays++;
                        break;
                }
            }
            else
            {
                day.StatusCode = date <= today
                    ? HrCalendarStatusConst.Worked
                    : HrCalendarStatusConst.PlannedWork;
                day.StatusName = date <= today ? "Worked" : "Planned work";
                if (date <= today)
                {
                    day.WorkedHours = plannedHours;
                    result.Summary.WorkedDays++;
                    result.Summary.WorkedHours += plannedHours;
                }
                else
                {
                    result.Summary.PlannedWorkDays++;
                    result.Summary.PlannedWorkHours += plannedHours;
                }
            }

            result.Days.Add(day);
        }

        return result;
    }

    private static short ToIsoDayOfWeek(DateOnly date) =>
        (short)(((int)date.DayOfWeek + 6) % 7 + 1);

    private static decimal GetFallbackHours(PayEmployment employment, short dayOfWeek) =>
        dayOfWeek <= 5 ? decimal.Round(employment.WeeklyHours / 5m, 2) : 0m;
}
