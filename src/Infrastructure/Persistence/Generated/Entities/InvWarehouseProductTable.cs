using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_warehouse_product_table")]
[Index("ProductTableStatusId", Name = "idx_inv_warehouse_product_table_status_id")]
[Index("WarehouseId", Name = "idx_inv_warehouse_product_table_warehouse_id")]
public partial class InvWarehouseProductTable
{
    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Key]
    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("product_table_status_id")]
    public short ProductTableStatusId { get; set; }

    [Column("received_date", TypeName = "timestamp without time zone")]
    public DateTime ReceivedDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("ProductTableId")]
    [InverseProperty("InvWarehouseProductTable")]
    public virtual InvProductTable ProductTable { get; set; } = null!;

    [ForeignKey("ProductTableStatusId")]
    [InverseProperty("InvWarehouseProductTables")]
    public virtual CmnProductTableStatus ProductTableStatus { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvWarehouseProductTables")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
