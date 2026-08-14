using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_commissioning_doc")]
[Index("OrganizationId", "DocDate", Name = "ix_fa_commissioning_doc_org_date")]
[Index("StatusId", Name = "ix_fa_commissioning_doc_status_id")]
[Index("OrganizationId", "DocNumber", Name = "uq_fa_commissioning_doc_org_number", IsUnique = true)]
public partial class FaCommissioningDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("doc_number")]
    [StringLength(50)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

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
    [InverseProperty("FaCommissioningDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("FaCommissioningDocCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [InverseProperty("CommissioningDoc")]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLines { get; set; } = new List<FaCommissioningDocLine>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("FaCommissioningDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("FaCommissioningDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("FaCommissioningDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaCommissioningDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("FaCommissioningDocUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
