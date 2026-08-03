using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_disposal_doc")]
public partial class FaDisposalDoc
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

    [Column("disposal_date", TypeName = "timestamp without time zone")]
    public DateTime DisposalDate { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("disposal_type_id")]
    public short DisposalTypeId { get; set; }

    [Column("reason")]
    [StringLength(500)]
    public string? Reason { get; set; }

    [Column("disposal_account_id")]
    public int? DisposalAccountId { get; set; }

    [Column("customer_account_id")]
    public int? CustomerAccountId { get; set; }

    [Column("vat_account_id")]
    public int? VatAccountId { get; set; }

    [Column("gain_account_id")]
    public int? GainAccountId { get; set; }

    [Column("loss_account_id")]
    public int? LossAccountId { get; set; }

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

    [ForeignKey(nameof(DisposalAccountId))]
    [InverseProperty(nameof(ChartAccount.FaDisposalDocDisposalAccounts))]
    public virtual ChartAccount? DisposalAccount { get; set; }

    [ForeignKey(nameof(CustomerAccountId))]
    [InverseProperty(nameof(ChartAccount.FaDisposalDocCustomerAccounts))]
    public virtual ChartAccount? CustomerAccount { get; set; }

    [ForeignKey(nameof(VatAccountId))]
    [InverseProperty(nameof(ChartAccount.FaDisposalDocVatAccounts))]
    public virtual ChartAccount? VatAccount { get; set; }

    [ForeignKey(nameof(GainAccountId))]
    [InverseProperty(nameof(ChartAccount.FaDisposalDocGainAccounts))]
    public virtual ChartAccount? GainAccount { get; set; }

    [ForeignKey(nameof(DisposalTypeId))]
    [InverseProperty(nameof(FaDisposalType.FaDisposalDocs))]
    public virtual FaDisposalType DisposalType { get; set; } = null!;

    [ForeignKey(nameof(LossAccountId))]
    [InverseProperty(nameof(ChartAccount.FaDisposalDocLossAccounts))]
    public virtual ChartAccount? LossAccount { get; set; }

    [ForeignKey("CreatedByUserId")]
    public virtual User? CreatedByUser { get; set; }

    [InverseProperty("DisposalDoc")]
    public virtual ICollection<FaDisposalDocLine> Lines { get; set; } = new List<FaDisposalDocLine>();

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
