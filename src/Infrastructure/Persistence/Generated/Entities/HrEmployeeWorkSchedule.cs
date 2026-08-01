using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("hr_employee_work_schedule")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_hr_employee_work_schedule_dates")]
[Index("OrganizationId", "EmployeeId", Name = "idx_hr_employee_work_schedule_employee")]
public partial class HrEmployeeWorkSchedule
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("updated_by_user_id")]
    public int? UpdatedByUserId { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("HrEmployeeWorkScheduleCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("HrEmployeeWorkSchedules")]
    public virtual PayEmployee Employee { get; set; } = null!;

    [InverseProperty("Schedule")]
    public virtual ICollection<HrEmployeeWorkScheduleDay> HrEmployeeWorkScheduleDays { get; set; } = new List<HrEmployeeWorkScheduleDay>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("HrEmployeeWorkSchedules")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("HrEmployeeWorkSchedules")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("HrEmployeeWorkScheduleUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
