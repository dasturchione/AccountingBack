using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("hr_employee_work_schedule")]
[Index(nameof(OrganizationId), nameof(EmployeeId), Name = "idx_hr_employee_work_schedule_employee")]
[Index(nameof(EffectiveFrom), nameof(EffectiveTo), Name = "idx_hr_employee_work_schedule_dates")]
public sealed class HrEmployeeWorkSchedule
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

    [Column("effective_from", TypeName = "date")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to", TypeName = "date")]
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

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public PayEmployee Employee { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public State State { get; set; } = null!;

    public ICollection<HrEmployeeWorkScheduleDay> Days { get; set; } = [];
}
