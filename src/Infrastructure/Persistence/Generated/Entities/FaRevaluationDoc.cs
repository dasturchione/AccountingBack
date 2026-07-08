using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_revaluation_doc")]
[Index("RevaluationDate", Name = "idx_fa_revaluation_doc_date")]
[Index("StateId", Name = "idx_fa_revaluation_doc_state_id")]
[Index("StatusId", Name = "idx_fa_revaluation_doc_status_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_fa_revaluation_doc_org_doc_number", IsUnique = true)]
public partial class FaRevaluationDoc
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

    [Column("revaluation_date", TypeName = "timestamp without time zone")]
    public DateTime RevaluationDate { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("reason")]
    [StringLength(500)]
    public string? Reason { get; set; }

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
    [InverseProperty("FaRevaluationDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("FaRevaluationDocCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [InverseProperty("RevaluationDoc")]
    public virtual ICollection<FaRevaluationDocLine> FaRevaluationDocLines { get; set; } = new List<FaRevaluationDocLine>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("FaRevaluationDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("FaRevaluationDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("FaRevaluationDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaRevaluationDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("FaRevaluationDocUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
