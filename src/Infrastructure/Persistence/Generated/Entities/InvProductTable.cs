using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_table")]
[Index("CurrentWarehouseId", Name = "idx_inv_product_table_current_warehouse_id")]
[Index("OrganizationId", "CurrentWarehouseId", "StatusId", Name = "idx_inv_product_table_org_warehouse_status")]
[Index("OrganizationId", "CurrentWarehouseId", "StatusId", "ProductId", Name = "idx_inv_product_table_org_warehouse_status_product")]
[Index("StatusId", Name = "ix_inv_product_table_status_id")]
public partial class InvProductTable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("serial_number")]
    [StringLength(250)]
    public string? SerialNumber { get; set; }

    [Column("marking_number")]
    [StringLength(250)]
    public string? MarkingNumber { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("current_warehouse_id")]
    public int? CurrentWarehouseId { get; set; }

    [ForeignKey("CurrentWarehouseId")]
    [InverseProperty("InvProductTables")]
    public virtual InvWarehouse? CurrentWarehouse { get; set; }

    [InverseProperty("SourceProductTable")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvInventoryAdjustmentDocTable> InvInventoryAdjustmentDocTables { get; set; } = new List<InvInventoryAdjustmentDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvInventoryCountDocTable> InvInventoryCountDocTables { get; set; } = new List<InvInventoryCountDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<InvTransferDocTable> InvTransferDocTables { get; set; } = new List<InvTransferDocTable>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvProductTables")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvProductTables")]
    public virtual InvProduct Product { get; set; } = null!;

    [InverseProperty("ProductTable")]
    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [ForeignKey("StateId")]
    [InverseProperty("InvProductTables")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("InvProductTables")]
    public virtual CmnProductTableStatus Status { get; set; } = null!;
}
