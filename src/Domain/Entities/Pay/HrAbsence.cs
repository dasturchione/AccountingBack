using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("hr_absence")]
[Index(nameof(OrganizationId), nameof(DocNumber), Name = "ux_hr_absence_org_doc_number", IsUnique = true)]
[Index(nameof(OrganizationId), nameof(EmployeeId), Name = "idx_hr_absence_employee")]
[Index(nameof(StartDate), nameof(EndDate), Name = "idx_hr_absence_dates")]
public sealed class HrAbsence
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("absence_type_id")]
    public short AbsenceTypeId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "date")]
    public DateOnly DocDate { get; set; }

    [Column("start_date", TypeName = "date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date", TypeName = "date")]
    public DateOnly EndDate { get; set; }

    [Column("note")]
    [StringLength(1000)]
    public string? Note { get; set; }

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

    [ForeignKey(nameof(AbsenceTypeId))]
    public HrAbsenceType AbsenceType { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public State State { get; set; } = null!;

    public ICollection<HrAbsenceAttachment> Attachments { get; set; } = [];
}
