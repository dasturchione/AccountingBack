using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_table")]
public partial class InvProductTable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("serial_number")]
    [StringLength(250)]
    public string? SerialNumber { get; set; }

    [Column("marking_number")]
    [StringLength(250)]
    public string? MarkingNumber { get; set; }

    [InverseProperty("SourceProductTable")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvInventoryAdjustmentDocTable> InvInventoryAdjustmentDocTables { get; set; } = new List<InvInventoryAdjustmentDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvInventoryCountDocTable> InvInventoryCountDocTables { get; set; } = new List<InvInventoryCountDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvOpeningInventoryTable> InvOpeningInventoryTables { get; set; } = new List<InvOpeningInventoryTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvTransferDocTable> InvTransferDocTables { get; set; } = new List<InvTransferDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvWarehouseProductBatchTable> InvWarehouseProductBatchTables { get; set; } = new List<InvWarehouseProductBatchTable>();

    [InverseProperty("ProductTable")]
    public virtual InvWarehouseProductTable? InvWarehouseProductTable { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("InvProductTables")]
    public virtual InvProduct Product { get; set; } = null!;

    [InverseProperty("ProductTable")]
    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<SaleShipmentTable> SaleShipmentTables { get; set; } = new List<SaleShipmentTable>();
}
