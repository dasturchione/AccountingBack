using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_table")]
public partial class ProductTable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("serial_number")]
    [StringLength(250)]
    public string? SerialNumber { get; set; }

    [Column("marking_number")]
    [StringLength(250)]
    public string? MarkingNumber { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("SourceProductTable")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty(nameof(WarehouseProductBatchTable.ProductTable))]
    public virtual ICollection<WarehouseProductBatchTable> WarehouseProductBatchTables { get; set; } = new List<WarehouseProductBatchTable>();

    [ForeignKey("ProductId")]
    [InverseProperty("ProductTables")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty(nameof(WarehouseProductTable.ProductTable))]
    public virtual WarehouseProductTable? WarehouseProductTable { get; set; }

    [InverseProperty("ProductTable")]
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();
}
