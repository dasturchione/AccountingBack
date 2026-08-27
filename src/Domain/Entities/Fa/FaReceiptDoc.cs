using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_receipt_doc")]
[Index(nameof(CounterpartyId), Name = "ix_fa_receipt_doc_counterparty_id")]
[Index(nameof(OrganizationId), nameof(DocDate), Name = "ix_fa_receipt_doc_org_date")]
[Index(nameof(ReceiptTypeId), Name = "ix_fa_receipt_doc_receipt_type_id")]
[Index(nameof(StatusId), Name = "ix_fa_receipt_doc_status_id")]
[Index(nameof(SupplierAccountId), Name = "ix_fa_receipt_doc_supplier_account_id")]
public partial class FaReceiptDoc
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

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("total_amount")]
    [Precision(24, 8)]
    public decimal TotalAmount { get; set; }

    [Column("vat_amount")]
    [Precision(24, 8)]
    public decimal VatAmount { get; set; }

    [Column("final_amount")]
    [Precision(24, 8)]
    public decimal FinalAmount { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("receipt_type_id")]
    public short ReceiptTypeId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime UpdatedDate { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [Column("supplier_account_id")]
    public int? SupplierAccountId { get; set; }

    [ForeignKey(nameof(CancelledByUserId))]
    [InverseProperty(nameof(User.FaReceiptDocCancelledByUsers))]
    public virtual User? CancelledByUser { get; set; }

    [ForeignKey("CounterpartyId")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CurrencyId")]
    public virtual Currency Currency { get; set; } = null!;

    [InverseProperty(nameof(FaReceiptDocLine.ReceiptDoc))]
    public virtual ICollection<FaReceiptDocLine> Lines { get; set; } = new List<FaReceiptDocLine>();

    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PostedByUserId))]
    [InverseProperty(nameof(User.FaReceiptDocPostedByUsers))]
    public virtual User? PostedByUser { get; set; }

    [ForeignKey(nameof(ReceiptTypeId))]
    [InverseProperty(nameof(FaReceiptType.FaReceiptDocs))]
    public virtual FaReceiptType ReceiptType { get; set; } = null!;

    [ForeignKey("StateId")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(SupplierAccountId))]
    [InverseProperty(nameof(ChartAccount.FaReceiptDocs))]
    public virtual ChartAccount? SupplierAccount { get; set; }
}
