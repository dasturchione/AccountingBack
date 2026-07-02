using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pur_doc_table")]
[Index("OwnerId", "Id", Name = "ix_pur_doc_table_owner_id_id")]
[Index("OwnerId", "ProductTableId", Name = "ux_pur_doc_table_owner_id_product_table_id", IsUnique = true)]
public partial class PurchaseDocTable
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
    [InverseProperty("PurchaseDocTables")]
    public virtual PurchaseDocProduct Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("PurchaseDocTables")]
    public virtual ProductTable ProductTable { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("PurchaseDocTables")]
    public virtual VatRate? VatRate { get; set; }
}
