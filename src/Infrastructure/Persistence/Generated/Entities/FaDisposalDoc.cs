using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_disposal_doc")]
[Index("DisposalDate", Name = "idx_fa_disposal_doc_date")]
[Index("DisposalTypeId", Name = "idx_fa_disposal_doc_disposal_type_id")]
[Index("StateId", Name = "idx_fa_disposal_doc_state_id")]
[Index("StatusId", Name = "idx_fa_disposal_doc_status_id")]
[Index("CustomerAccountId", Name = "ix_fa_disposal_doc_customer_account")]
[Index("DisposalAccountId", Name = "ix_fa_disposal_doc_disposal_account")]
[Index("GainAccountId", Name = "ix_fa_disposal_doc_gain_account")]
[Index("LossAccountId", Name = "ix_fa_disposal_doc_loss_account")]
[Index("VatAccountId", Name = "ix_fa_disposal_doc_vat_account")]
[Index("OrganizationId", "DocNumber", Name = "ux_fa_disposal_doc_org_doc_number", IsUnique = true)]
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

    [Column("disposal_type_id")]
    public short? DisposalTypeId { get; set; }

    [ForeignKey("CancelledByUserId")]
    [InverseProperty("FaDisposalDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("FaDisposalDocCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [ForeignKey("CustomerAccountId")]
    [InverseProperty("FaDisposalDocCustomerAccounts")]
    public virtual AccChartAccount? CustomerAccount { get; set; }

    [ForeignKey("DisposalAccountId")]
    [InverseProperty("FaDisposalDocDisposalAccounts")]
    public virtual AccChartAccount? DisposalAccount { get; set; }

    [ForeignKey("DisposalTypeId")]
    [InverseProperty("FaDisposalDocs")]
    public virtual FaDisposalType? DisposalType { get; set; }

    [InverseProperty("DisposalDoc")]
    public virtual ICollection<FaDisposalDocLine> FaDisposalDocLines { get; set; } = new List<FaDisposalDocLine>();

    [ForeignKey("GainAccountId")]
    [InverseProperty("FaDisposalDocGainAccounts")]
    public virtual AccChartAccount? GainAccount { get; set; }

    [ForeignKey("LossAccountId")]
    [InverseProperty("FaDisposalDocLossAccounts")]
    public virtual AccChartAccount? LossAccount { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("FaDisposalDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("FaDisposalDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("FaDisposalDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaDisposalDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("FaDisposalDocUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }

    [ForeignKey("VatAccountId")]
    [InverseProperty("FaDisposalDocVatAccounts")]
    public virtual AccChartAccount? VatAccount { get; set; }
}
