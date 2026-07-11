using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pur_doc_product")]
[Index("DebitAccountId", Name = "idx_pur_doc_product_debit_account_id")]
[Index("VatAccountId", Name = "idx_pur_doc_product_vat_account_id")]
[Index("OwnerId", Name = "ix_pur_doc_product_owner_id")]
public partial class PurDocProduct
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("vat_rate_id")]
    public short? VatRateId { get; set; }

    [Column("vat_amount")]
    [Precision(24, 8)]
    public decimal VatAmount { get; set; }

    [Column("total_amount")]
    [Precision(24, 8)]
    public decimal TotalAmount { get; set; }

    [Column("unit_price")]
    [Precision(24, 8)]
    public decimal UnitPrice { get; set; }

    [Column("debit_account_id")]
    public int? DebitAccountId { get; set; }

    [Column("vat_account_id")]
    public int? VatAccountId { get; set; }

    [ForeignKey("DebitAccountId")]
    [InverseProperty("PurDocProductDebitAccounts")]
    public virtual AccChartAccount? DebitAccount { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("PurDocProducts")]
    public virtual PurDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("PurDocProducts")]
    public virtual InvProduct Product { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    [ForeignKey("UnitId")]
    [InverseProperty("PurDocProducts")]
    public virtual CmnUnit Unit { get; set; } = null!;

    [ForeignKey("VatAccountId")]
    [InverseProperty("PurDocProductVatAccounts")]
    public virtual AccChartAccount? VatAccount { get; set; }

    [ForeignKey("VatRateId")]
    [InverseProperty("PurDocProducts")]
    public virtual CmnVatRate? VatRate { get; set; }
}
