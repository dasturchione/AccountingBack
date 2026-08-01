using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_receipt_doc")]
[Index("CounterpartyId", Name = "idx_fa_receipt_doc_counterparty_id")]
[Index("CurrencyId", Name = "idx_fa_receipt_doc_currency_id")]
[Index("DocDate", Name = "idx_fa_receipt_doc_doc_date")]
[Index("StateId", Name = "idx_fa_receipt_doc_state_id")]
[Index("StatusId", Name = "idx_fa_receipt_doc_status_id")]
[Index("WarehouseId", Name = "idx_fa_receipt_doc_warehouse_id")]
[Index("SupplierAccountId", Name = "ix_fa_receipt_doc_supplier_account")]
[Index("OrganizationId", "DocNumber", Name = "ux_fa_receipt_doc_org_doc_number", IsUnique = true)]
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
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("warehouse_id")]
    public int? WarehouseId { get; set; }

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

    [Column("receipt_type")]
    [StringLength(50)]
    public string ReceiptType { get; set; } = null!;

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

    [ForeignKey("CounterpartyId")]
    [InverseProperty("FaReceiptDocs")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("FaReceiptDocs")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<FaReceiptDocLine> FaReceiptDocLines { get; set; } = new List<FaReceiptDocLine>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("FaReceiptDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("FaReceiptDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaReceiptDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("SupplierAccountId")]
    [InverseProperty("FaReceiptDocs")]
    public virtual AccChartAccount? SupplierAccount { get; set; }

    [ForeignKey("WarehouseId")]
    [InverseProperty("FaReceiptDocs")]
    public virtual InvWarehouse? Warehouse { get; set; }
}
