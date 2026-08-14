using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("rtl_sale_doc")]
[Index("CashRegisterId", Name = "ix_rtl_sale_doc_cash_register_id")]
[Index("CounterpartyId", Name = "ix_rtl_sale_doc_counterparty_id")]
[Index("CurrencyId", Name = "ix_rtl_sale_doc_currency_id")]
[Index("DocDate", Name = "ix_rtl_sale_doc_doc_date")]
[Index("OrganizationId", "DocDate", Name = "ix_rtl_sale_doc_org_doc_date")]
[Index("OrganizationId", Name = "ix_rtl_sale_doc_organization_id")]
[Index("ReceivableAccountId", Name = "ix_rtl_sale_doc_receivable_account_id")]
[Index("StateId", Name = "ix_rtl_sale_doc_state_id")]
[Index("StatusId", Name = "ix_rtl_sale_doc_status_id")]
[Index("VatAccountId", Name = "ix_rtl_sale_doc_vat_account_id")]
[Index("WarehouseId", Name = "ix_rtl_sale_doc_warehouse_id")]
public partial class RtlSaleDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("cash_register_id")]
    public int CashRegisterId { get; set; }

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

    [Column("receivable_account_id")]
    public int? ReceivableAccountId { get; set; }

    [Column("vat_account_id")]
    public int? VatAccountId { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("exchange_rate")]
    [Precision(18, 6)]
    public decimal ExchangeRate { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey("CancelledByUserId")]
    [InverseProperty("RtlSaleDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CashRegisterId")]
    [InverseProperty("RtlSaleDocs")]
    public virtual FiscalCashRegister CashRegister { get; set; } = null!;

    [ForeignKey("CounterpartyId")]
    [InverseProperty("RtlSaleDocs")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("RtlSaleDocs")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("RtlSaleDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("RtlSaleDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("ReceivableAccountId")]
    [InverseProperty("RtlSaleDocReceivableAccounts")]
    public virtual AccChartAccount? ReceivableAccount { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<RtlSaleDocPayment> RtlSaleDocPayments { get; set; } = new List<RtlSaleDocPayment>();

    [InverseProperty("Owner")]
    public virtual ICollection<RtlSaleDocProduct> RtlSaleDocProducts { get; set; } = new List<RtlSaleDocProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("RtlSaleDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("RtlSaleDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("VatAccountId")]
    [InverseProperty("RtlSaleDocVatAccounts")]
    public virtual AccChartAccount? VatAccount { get; set; }

    [ForeignKey("WarehouseId")]
    [InverseProperty("RtlSaleDocs")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
