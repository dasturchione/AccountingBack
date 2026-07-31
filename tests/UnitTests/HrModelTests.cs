using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class HrModelTests
{
    [Fact]
    public void EfModel_ContainsHrSchedulesAbsencesAttachmentsAndPersonalTimesheetNorm()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_validation;Username=model_validation;Password=model_validation")
            .Options;

        using var context = new AppDbContext(options);

        Assert.Equal(
            "hr_employee_work_schedule",
            context.Model.FindEntityType(typeof(HrEmployeeWorkSchedule))?.GetTableName());
        Assert.Equal(
            "hr_employee_work_schedule_day",
            context.Model.FindEntityType(typeof(HrEmployeeWorkScheduleDay))?.GetTableName());
        Assert.Equal(
            "hr_absence",
            context.Model.FindEntityType(typeof(HrAbsence))?.GetTableName());
        Assert.Equal(
            "hr_absence_attachment",
            context.Model.FindEntityType(typeof(HrAbsenceAttachment))?.GetTableName());

        var timesheetLine = context.Model.FindEntityType(typeof(PayTimesheetLine));
        Assert.Equal(
            "norm_work_days",
            timesheetLine?.FindProperty(nameof(PayTimesheetLine.NormWorkDays))?.GetColumnName());
        Assert.Equal(
            "norm_work_hours",
            timesheetLine?.FindProperty(nameof(PayTimesheetLine.NormWorkHours))?.GetColumnName());
    }
}
