using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pur_doc_product")]
public partial class PurchaseDocProduct
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
    [InverseProperty(nameof(ChartAccount.PurchaseDocProductDebitAccounts))]
    public virtual ChartAccount? DebitAccount { get; set; }

    [ForeignKey("VatAccountId")]
    [InverseProperty(nameof(ChartAccount.PurchaseDocProductVatAccounts))]
    public virtual ChartAccount? VatAccount { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("PurchaseDocProducts")]
    public virtual PurchaseDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("PurchaseDocProducts")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();

    [ForeignKey("UnitId")]
    [InverseProperty("PurchaseDocProducts")]
    public virtual Unit Unit { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("PurchaseDocProducts")]
    public virtual VatRate? VatRate { get; set; }
}
