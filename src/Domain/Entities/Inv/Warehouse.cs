using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_warehouse")]
public partial class Warehouse
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("branch_id")]
    public int? BranchId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string? Code { get; set; }

    [Column("address")]
    [StringLength(1000)]
    public string? Address { get; set; }

    [Column("is_main")]
    public bool IsMain { get; set; }

    [ForeignKey("BranchId")]
    [InverseProperty("Warehouses")]
    public virtual Branch? Branch { get; set; }

    [InverseProperty("Warehouse")]
    public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();

    [InverseProperty(nameof(WarehouseProductTable.Warehouse))]
    public virtual ICollection<WarehouseProductTable> WarehouseProductTables { get; set; } = new List<WarehouseProductTable>();

    [InverseProperty(nameof(SaleShipmentDoc.Warehouse))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocs { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(WarehouseProductMovement.Warehouse))]
    public virtual ICollection<WarehouseProductMovement> WarehouseProductMovements { get; set; } = new List<WarehouseProductMovement>();

    [InverseProperty(nameof(WarehouseProductBatch.Warehouse))]
    public virtual ICollection<WarehouseProductBatch> WarehouseProductBatches { get; set; } = new List<WarehouseProductBatch>();

    [InverseProperty(nameof(OpeningInventory.Warehouse))]
    public virtual ICollection<OpeningInventory> OpeningInventories { get; set; } = new List<OpeningInventory>();

    [InverseProperty("SourceWarehouse")]
    public virtual ICollection<WarehouseTransferDoc> SourceWarehouseTransferDocs { get; set; } = new List<WarehouseTransferDoc>();

    [InverseProperty("DestinationWarehouse")]
    public virtual ICollection<WarehouseTransferDoc> DestinationWarehouseTransferDocs { get; set; } = new List<WarehouseTransferDoc>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("Warehouses")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("Warehouse")]
    public virtual ICollection<PurchaseDoc> PurDocs { get; set; } = new List<PurchaseDoc>();

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("Warehouses")]
    public virtual User? ResponsibleUser { get; set; }

    [InverseProperty("Warehouse")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("Warehouses")]
    public virtual State State { get; set; } = null!;
}
