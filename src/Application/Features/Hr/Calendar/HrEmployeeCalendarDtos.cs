namespace Application.Features.Hr.Calendar;

public sealed class HrEmployeeCalendarDayDto
{
    public DateOnly Date { get; set; }
    public short DayOfWeek { get; set; }
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

public sealed class HrEmployeeCalendarSummaryDto
{
    public decimal NormWorkDays { get; set; }
    public decimal NormWorkHours { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal PlannedWorkDays { get; set; }
    public decimal PlannedWorkHours { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal SickDays { get; set; }
    public decimal AbsentDays { get; set; }
}

public sealed class HrEmployeeCalendarDto
{
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public HrEmployeeCalendarSummaryDto Summary { get; set; } = new();
    public List<HrEmployeeCalendarDayDto> Days { get; set; } = [];
}
