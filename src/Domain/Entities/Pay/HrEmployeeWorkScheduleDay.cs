using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("hr_employee_work_schedule_day")]
[Index(nameof(OrganizationId), Name = "idx_hr_employee_work_schedule_day_organization")]
[Index(nameof(ScheduleId), Name = "idx_hr_employee_work_schedule_day_schedule")]
public sealed class HrEmployeeWorkScheduleDay
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("schedule_id")]
    public long ScheduleId { get; set; }

    [Column("day_of_week")]
    public short DayOfWeek { get; set; }

    [Column("work_hours")]
    [Precision(5, 2)]
    public decimal WorkHours { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(ScheduleId))]
    public HrEmployeeWorkSchedule Schedule { get; set; } = null!;
}
