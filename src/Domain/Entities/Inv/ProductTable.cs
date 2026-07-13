using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_table")]
[Index("StatusId", Name = "ix_inv_product_table_status_id")]
[Index("CurrentWarehouseId", Name = "idx_inv_product_table_current_warehouse_id")]
[Index("OrganizationId", "CurrentWarehouseId", "StatusId", Name = "idx_inv_product_table_org_warehouse_status")]
[Index("OrganizationId", "CurrentWarehouseId", "StatusId", "ProductId", Name = "idx_inv_product_table_org_warehouse_status_product")]
public partial class ProductTable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("current_warehouse_id")]
    public int? CurrentWarehouseId { get; set; }

    [Column("serial_number")]
    [StringLength(250)]
    public string? SerialNumber { get; set; }

    [Column("marking_number")]
    [StringLength(250)]
    public string? MarkingNumber { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("SourceProductTable")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("ProductTables")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("CurrentWarehouseId")]
    [InverseProperty("CurrentProductTables")]
    public virtual Warehouse? CurrentWarehouse { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductTables")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty("ProductTable")]
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [ForeignKey("StateId")]
    [InverseProperty("ProductTables")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("ProductTables")]
    public virtual ProductTableStatus Status { get; set; } = null!;

}
