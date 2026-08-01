using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("hr_employee_work_schedule_day")]
[Index("OrganizationId", Name = "idx_hr_employee_work_schedule_day_organization")]
[Index("ScheduleId", Name = "idx_hr_employee_work_schedule_day_schedule")]
[Index("ScheduleId", "DayOfWeek", Name = "ux_hr_employee_work_schedule_day", IsUnique = true)]
public partial class HrEmployeeWorkScheduleDay
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

    [ForeignKey("OrganizationId")]
    [InverseProperty("HrEmployeeWorkScheduleDays")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ScheduleId")]
    [InverseProperty("HrEmployeeWorkScheduleDays")]
    public virtual HrEmployeeWorkSchedule Schedule { get; set; } = null!;
}
