namespace Application.Features.Hr.Schedules;

public sealed class HrWorkScheduleDaySaveDto
{
    public short DayOfWeek { get; set; }
    public decimal WorkHours { get; set; }
}

public class HrWorkScheduleSaveDto
{
    public string Name { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public List<HrWorkScheduleDaySaveDto> Days { get; set; } = [];
}

public sealed class HrWorkScheduleDto : HrWorkScheduleSaveDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public decimal WeeklyHours { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
