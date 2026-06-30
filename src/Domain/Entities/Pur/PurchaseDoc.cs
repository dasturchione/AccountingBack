using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pur_doc")]
[Index("ContractId", Name = "idx_pur_doc_contract_id")]
[Index("CounterpartyId", Name = "idx_pur_doc_counterparty_id")]
[Index("DocDate", Name = "idx_pur_doc_doc_date")]
[Index("OrganizationId", Name = "idx_pur_doc_organization_id")]
[Index("StateId", Name = "idx_pur_doc_state_id")]
[Index("StatusId", Name = "idx_pur_doc_status_id")]
[Index("WarehouseId", Name = "idx_pur_doc_warehouse_id")]
[Index("CancelledByUserId", Name = "idx_pur_doc_cancelled_by_user_id")]
[Index("PostedByUserId", Name = "idx_pur_doc_posted_by_user_id")]
public partial class PurchaseDoc
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
    [InverseProperty("PurDocs")]
    public virtual Contract? Contract { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("PurDocs")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty("PurchaseDocs")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("PurchaseDocs")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProducts { get; set; } = new List<PurchaseDocProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("PurDocs")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("PurchaseDocs")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("PurDocs")]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
