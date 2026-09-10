using Domain.Entities;

namespace Application.Features.Pay.PayrollDocuments;

/// <summary>
/// Copies attendance quantities from a posted timesheet into a payroll-line snapshot.
/// Keeping this as a pure mapper prevents reports from reading mutable timesheet rows.
/// </summary>
public readonly record struct PayrollAttendanceSnapshot(
    decimal WorkedDays,
    decimal WorkedHours,
    decimal PaidLeaveDays,
    decimal PaidSickDays,
    decimal OvertimeHours,
    decimal NightHours,
    decimal HolidayHours,
    decimal WeekendHours);

public static class PayrollAttendanceSnapshotCalculator
{
    public static PayrollAttendanceSnapshot FromTimesheet(PayTimesheetLine line) => new(
        line.WorkedDays,
        line.WorkedHours,
        line.PaidLeaveDays,
        line.PaidSickDays,
        line.OvertimeHours,
        line.NightHours,
        line.HolidayHours,
        line.WeekendHours);
}
