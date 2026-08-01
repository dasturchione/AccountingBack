using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("hr_absence")]
[Index("StartDate", "EndDate", Name = "idx_hr_absence_dates")]
[Index("OrganizationId", "EmployeeId", Name = "idx_hr_absence_employee")]
[Index("AbsenceTypeId", Name = "idx_hr_absence_type")]
[Index("OrganizationId", "DocNumber", Name = "ux_hr_absence_org_doc_number", IsUnique = true)]
public partial class HrAbsence
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

    [Column("doc_date")]
    public DateOnly DocDate { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
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

    [ForeignKey("AbsenceTypeId")]
    [InverseProperty("HrAbsences")]
    public virtual HrAbsenceType AbsenceType { get; set; } = null!;

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("HrAbsenceCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("HrAbsences")]
    public virtual PayEmployee Employee { get; set; } = null!;

    [InverseProperty("Absence")]
    public virtual ICollection<HrAbsenceAttachment> HrAbsenceAttachments { get; set; } = new List<HrAbsenceAttachment>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("HrAbsences")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("HrAbsences")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("HrAbsenceUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
