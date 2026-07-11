using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sale_doc")]
public partial class SaleDoc
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
    public int CounterpartyId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

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

    [Column("customer_account_id")]
    public int? CustomerAccountId { get; set; }

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

    [Column("contract_id")]
    public long? ContractId { get; set; }

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
    [ForeignKey("ContractId")]
    [InverseProperty("SaleDocs")]
    public virtual Contract? Contract { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("SaleDocs")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty("SaleDocs")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("SaleDocs")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [ForeignKey("CustomerAccountId")]
    [InverseProperty(nameof(ChartAccount.SaleDocCustomerAccounts))]
    public virtual ChartAccount? CustomerAccount { get; set; }

    [ForeignKey("VatAccountId")]
    [InverseProperty(nameof(ChartAccount.SaleDocVatAccounts))]
    public virtual ChartAccount? VatAccount { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("SaleDocs")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("SaleDocs")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("SaleDocs")]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
