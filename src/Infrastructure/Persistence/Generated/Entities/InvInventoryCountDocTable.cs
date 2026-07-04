using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_inventory_count_doc_table")]
[Index("ProductTableId", Name = "idx_inv_inventory_count_doc_table_product_table_id")]
[Index("OwnerId", Name = "ix_inv_inventory_count_doc_table_owner_id")]
[Index("OwnerId", "ProductTableId", Name = "ux_inv_inventory_count_doc_table_owner_product_table", IsUnique = true)]
public partial class InvInventoryCountDocTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_table_id")]
    public int? ProductTableId { get; set; }

    [Column("barcode")]
    [StringLength(100)]
    public string? Barcode { get; set; }

    [Column("serial_number")]
    [StringLength(250)]
    public string? SerialNumber { get; set; }

    [Column("marking_number")]
    [StringLength(250)]
    public string? MarkingNumber { get; set; }

    [Column("cost_price")]
    [Precision(24, 8)]
    public decimal CostPrice { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("InvInventoryCountDocTables")]
    public virtual InvInventoryCountLine Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("InvInventoryCountDocTables")]
    public virtual InvProductTable? ProductTable { get; set; }
}
