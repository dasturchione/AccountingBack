using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pur_doc_table")]
[Index("ItemTypeId", Name = "idx_pur_doc_table_item_type_id")]
[Index("OwnerId", Name = "idx_pur_doc_table_owner_id")]
[Index("ProductTableId", Name = "idx_pur_doc_table_product_id")]
[Index("ServiceId", Name = "idx_pur_doc_table_service_id")]
[Index("VatRateId", Name = "idx_pur_doc_table_vat_rate_id")]
public partial class PurDocTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_table_id")]
    public int? ProductTableId { get; set; }

    [Column("quantity")]
    [Precision(18, 3)]
    public decimal Quantity { get; set; }

    [Column("price")]
    [Precision(18, 2)]
    public decimal Price { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("vat_rate_id")]
    public short? VatRateId { get; set; }

    [Column("vat_amount")]
    [Precision(18, 2)]
    public decimal VatAmount { get; set; }

    [Column("total_amount")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [Column("item_type_id")]
    public short ItemTypeId { get; set; }

    [Column("service_id")]
    public long? ServiceId { get; set; }

    [ForeignKey("ItemTypeId")]
    [InverseProperty("PurDocTables")]
    public virtual CmnPurchaseItemType ItemType { get; set; } = null!;

    [ForeignKey("OwnerId")]
    [InverseProperty("PurDocTables")]
    public virtual PurDoc Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("PurDocTables")]
    public virtual InvProductTable? ProductTable { get; set; }

    [ForeignKey("ServiceId")]
    [InverseProperty("PurDocTables")]
    public virtual PurService? Service { get; set; }

    [ForeignKey("VatRateId")]
    [InverseProperty("PurDocTables")]
    public virtual CmnVatRate? VatRate { get; set; }
}
