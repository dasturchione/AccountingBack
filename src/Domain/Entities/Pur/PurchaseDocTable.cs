using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pur_doc_table")]
[Index("ExpenseAccountId", Name = "idx_pur_doc_table_expense_account_id")]
[Index("ItemTypeId", Name = "idx_pur_doc_table_item_type_id")]
[Index("OwnerId", Name = "idx_pur_doc_table_owner_id")]
[Index("ProductTableId", Name = "idx_pur_doc_table_product_id")]
[Index("ServiceId", Name = "idx_pur_doc_table_service_id")]
[Index("VatRateId", Name = "idx_pur_doc_table_vat_rate_id")]
public partial class PurchaseDocTable
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
    public int? ServiceId { get; set; }

    [Column("service_name")]
    [StringLength(250)]
    public string? ServiceName { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [ForeignKey("ExpenseAccountId")]
    [InverseProperty("PurDocTables")]
    public virtual ChartAccount? ExpenseAccount { get; set; }

    [ForeignKey("ItemTypeId")]
    [InverseProperty("PurDocTables")]
    public virtual PurchaseItemType ItemType { get; set; } = null!;

    [ForeignKey("OwnerId")]
    [InverseProperty("PurDocTables")]
    public virtual PurchaseDoc Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("PurchaseDocTables")]
    public virtual ProductTable? ProductTable { get; set; }

    [ForeignKey("ServiceId")]
    [InverseProperty("PurchaseDocTables")]
    public virtual Product? Service { get; set; }

    [ForeignKey("VatRateId")]
    [InverseProperty("PurchaseDocTables")]
    public virtual VatRate? VatRate { get; set; }
}
