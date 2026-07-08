using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_movement_doc")]
[Index("DocDate", Name = "idx_fa_movement_doc_doc_date")]
[Index("FromDepartmentId", Name = "idx_fa_movement_doc_from_department_id")]
[Index("FromResponsibleUserId", Name = "idx_fa_movement_doc_from_responsible_user_id")]
[Index("StateId", Name = "idx_fa_movement_doc_state_id")]
[Index("StatusId", Name = "idx_fa_movement_doc_status_id")]
[Index("ToDepartmentId", Name = "idx_fa_movement_doc_to_department_id")]
[Index("ToResponsibleUserId", Name = "idx_fa_movement_doc_to_responsible_user_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_fa_movement_doc_org_doc_number", IsUnique = true)]
public partial class FaMovementDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("from_department_id")]
    public int? FromDepartmentId { get; set; }

    [Column("to_department_id")]
    public int? ToDepartmentId { get; set; }

    [Column("from_responsible_user_id")]
    public int? FromResponsibleUserId { get; set; }

    [Column("to_responsible_user_id")]
    public int? ToResponsibleUserId { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime UpdatedDate { get; set; }

    [Column("updated_by_user_id")]
    public int? UpdatedByUserId { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey("CancelledByUserId")]
    [InverseProperty("FaMovementDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("FaMovementDocCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [InverseProperty("MovementDoc")]
    public virtual ICollection<FaMovementDocLine> FaMovementDocLines { get; set; } = new List<FaMovementDocLine>();

    [ForeignKey("FromDepartmentId")]
    [InverseProperty("FaMovementDocFromDepartments")]
    public virtual OrgDepartment? FromDepartment { get; set; }

    [ForeignKey("FromResponsibleUserId")]
    [InverseProperty("FaMovementDocFromResponsibleUsers")]
    public virtual SysUser? FromResponsibleUser { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("FaMovementDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("FaMovementDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("FaMovementDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaMovementDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("ToDepartmentId")]
    [InverseProperty("FaMovementDocToDepartments")]
    public virtual OrgDepartment? ToDepartment { get; set; }

    [ForeignKey("ToResponsibleUserId")]
    [InverseProperty("FaMovementDocToResponsibleUsers")]
    public virtual SysUser? ToResponsibleUser { get; set; }

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("FaMovementDocUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
