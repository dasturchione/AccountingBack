using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_receipt_doc_line")]
[Index("OwnerId", Name = "idx_fa_receipt_doc_line_owner_id")]
[Index("SourceProductId", Name = "idx_fa_receipt_doc_line_source_product_id")]
[Index("VatRateId", Name = "idx_fa_receipt_doc_line_vat_rate_id")]
public partial class FaReceiptDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("source_product_id")]
    public int? SourceProductId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("price")]
    [Precision(24, 8)]
    public decimal Price { get; set; }

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

    [InverseProperty("Owner")]
    public virtual ICollection<FaReceiptDocAsset> Assets { get; set; } = new List<FaReceiptDocAsset>();

    [ForeignKey("OwnerId")]
    [InverseProperty("Lines")]
    public virtual FaReceiptDoc Owner { get; set; } = null!;

    [ForeignKey("SourceProductId")]
    public virtual Product? SourceProduct { get; set; }

    [ForeignKey("VatRateId")]
    public virtual VatRate? VatRate { get; set; }
}
