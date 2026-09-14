using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("rtl_sale_doc")]
public partial class RetailSaleDoc
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

        /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    [Column("price_includes_vat")]
    public bool PriceIncludesVat { get; set; }

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

    [ForeignKey(nameof(CancelledByUserId))]
    [InverseProperty(nameof(User.RetailSaleDocCancelledByUsers))]
    public virtual User? CancelledByUser { get; set; }

    [ForeignKey(nameof(CashRegisterId))]
    [InverseProperty(nameof(FiscalCashRegister.RetailSaleDocs))]
    public virtual FiscalCashRegister CashRegister { get; set; } = null!;

    [ForeignKey(nameof(CounterpartyId))]
    [InverseProperty(nameof(CounterpartyCard.RetailSaleDocs))]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey(nameof(CurrencyId))]
    [InverseProperty(nameof(Currency.RetailSaleDocs))]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.RetailSaleDocs))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PostedByUserId))]
    [InverseProperty(nameof(User.RetailSaleDocPostedByUsers))]
    public virtual User? PostedByUser { get; set; }

    [ForeignKey(nameof(ReceivableAccountId))]
    [InverseProperty(nameof(ChartAccount.RetailSaleDocReceivableAccounts))]
    public virtual ChartAccount? ReceivableAccount { get; set; }

    [InverseProperty(nameof(RetailSaleDocPayment.Owner))]
    public virtual ICollection<RetailSaleDocPayment> RetailSaleDocPayments { get; set; } = new List<RetailSaleDocPayment>();

    [InverseProperty(nameof(RetailSaleDocProduct.Owner))]
    public virtual ICollection<RetailSaleDocProduct> RetailSaleDocProducts { get; set; } = new List<RetailSaleDocProduct>();

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.RetailSaleDocs))]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(StatusId))]
    [InverseProperty(nameof(DocumentStatus.RetailSaleDocs))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(VatAccountId))]
    [InverseProperty(nameof(ChartAccount.RetailSaleDocVatAccounts))]
    public virtual ChartAccount? VatAccount { get; set; }

    [ForeignKey(nameof(WarehouseId))]
    [InverseProperty(nameof(Warehouse.RetailSaleDocs))]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
