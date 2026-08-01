using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_revaluation_doc")]
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

    [Column("revaluation_reserve_account_id")]
    public int? RevaluationReserveAccountId { get; set; }

    [Column("revaluation_loss_account_id")]
    public int? RevaluationLossAccountId { get; set; }

    [ForeignKey(nameof(RevaluationReserveAccountId))]
    [InverseProperty(nameof(ChartAccount.FaRevaluationDocRevaluationReserveAccounts))]
    public virtual ChartAccount? RevaluationReserveAccount { get; set; }

    [ForeignKey(nameof(RevaluationLossAccountId))]
    [InverseProperty(nameof(ChartAccount.FaRevaluationDocRevaluationLossAccounts))]
    public virtual ChartAccount? RevaluationLossAccount { get; set; }

    [ForeignKey("CreatedByUserId")]
    public virtual User? CreatedByUser { get; set; }

    [InverseProperty("RevaluationDoc")]
    public virtual ICollection<FaRevaluationDocLine> Lines { get; set; } = new List<FaRevaluationDocLine>();

    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    public virtual User? PostedByUser { get; set; }

    [ForeignKey("CancelledByUserId")]
    public virtual User? CancelledByUser { get; set; }

    [ForeignKey("StateId")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    public virtual User? UpdatedByUser { get; set; }
}
