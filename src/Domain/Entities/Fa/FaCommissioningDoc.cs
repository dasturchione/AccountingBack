using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_commissioning_doc")]
[Index(nameof(OrganizationId), nameof(DocDate), Name = "ix_fa_commissioning_doc_org_date")]
[Index(nameof(StatusId), Name = "ix_fa_commissioning_doc_status_id")]
[Index(nameof(OrganizationId), nameof(DocNumber), Name = "uq_fa_commissioning_doc_org_number", IsUnique = true)]
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

    [ForeignKey(nameof(CancelledByUserId))]
    [InverseProperty(nameof(User.FaCommissioningDocCancelledByUsers))]
    public virtual User? CancelledByUser { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    [InverseProperty(nameof(User.FaCommissioningDocCreatedByUsers))]
    public virtual User? CreatedByUser { get; set; }

    [InverseProperty(nameof(FaCommissioningDocLine.CommissioningDoc))]
    public virtual ICollection<FaCommissioningDocLine> Lines { get; set; } = new List<FaCommissioningDocLine>();

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.FaCommissioningDocs))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PostedByUserId))]
    [InverseProperty(nameof(User.FaCommissioningDocPostedByUsers))]
    public virtual User? PostedByUser { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.FaCommissioningDocs))]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(StatusId))]
    [InverseProperty(nameof(DocumentStatus.FaCommissioningDocs))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(UpdatedByUserId))]
    [InverseProperty(nameof(User.FaCommissioningDocUpdatedByUsers))]
    public virtual User? UpdatedByUser { get; set; }
}
