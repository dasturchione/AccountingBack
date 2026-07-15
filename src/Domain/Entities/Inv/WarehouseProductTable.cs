using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_warehouse_product_table")]
public partial class WarehouseProductTable
{
    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Key]
    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("product_table_status_id")]
    public short StatusId { get; set; }

    [Column("received_date", TypeName = "timestamp without time zone")]
    public DateTime ReceivedDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("ProductTableId")]
    [InverseProperty(nameof(ProductTable.WarehouseProductTable))]
    public virtual ProductTable ProductTable { get; set; } = null!;

    [ForeignKey(nameof(StatusId))]
    [InverseProperty(nameof(ProductTableStatus.WarehouseProductTables))]
    public virtual ProductTableStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty(nameof(Warehouse.WarehouseProductTables))]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
