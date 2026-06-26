using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pur_doc_table")]
[Index("OwnerId", "Id", Name = "ix_pur_doc_table_owner_id_id")]
[Index("OwnerId", "ProductTableId", Name = "ux_pur_doc_table_owner_id_product_table_id", IsUnique = true)]
public partial class PurDocTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_table_id")]
    public int ProductTableId { get; set; }

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

    [ForeignKey("OwnerId")]
    [InverseProperty("PurDocTables")]
    public virtual PurDocProduct Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("PurDocTables")]
    public virtual InvProductTable ProductTable { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("PurDocTables")]
    public virtual CmnVatRate? VatRate { get; set; }
}
