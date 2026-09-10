using Application.Features.Pay.PayrollDocuments;
using Domain.Entities;

namespace UnitTests;

public sealed class PayrollAttendanceSnapshotTests
{
    [Fact]
    public void Copies_work_and_special_attendance_totals_from_timesheet_line()
    {
        var line = new PayTimesheetLine
        {
            WorkedDays = 21m,
            WorkedHours = 164m,
            PaidLeaveDays = 2m,
            PaidSickDays = 1m,
            OvertimeHours = 4.5m,
            NightHours = 3m,
            HolidayHours = 8m,
            WeekendHours = 6m
        };

        var snapshot = PayrollAttendanceSnapshotCalculator.FromTimesheet(line);

        Assert.Equal(21m, snapshot.WorkedDays);
        Assert.Equal(164m, snapshot.WorkedHours);
        Assert.Equal(2m, snapshot.PaidLeaveDays);
        Assert.Equal(1m, snapshot.PaidSickDays);
        Assert.Equal(4.5m, snapshot.OvertimeHours);
        Assert.Equal(3m, snapshot.NightHours);
        Assert.Equal(8m, snapshot.HolidayHours);
        Assert.Equal(6m, snapshot.WeekendHours);
    }
}
