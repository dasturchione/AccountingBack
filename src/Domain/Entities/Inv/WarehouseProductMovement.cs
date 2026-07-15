using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("inv_warehouse_product_movement")]
public partial class WarehouseProductMovement
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("document_line_id")]
    public long? DocumentLineId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("movement_date", TypeName = "timestamp without time zone")]
    public DateTime MovementDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty(nameof(DocumentType.WarehouseProductMovements))]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [InverseProperty(nameof(WarehouseProductBatch.ReceiptMovement))]
    public virtual WarehouseProductBatch? WarehouseProductBatch { get; set; }

    [InverseProperty(nameof(WarehouseProductBatchAllocation.IssueMovement))]
    public virtual ICollection<WarehouseProductBatchAllocation> WarehouseProductBatchAllocations { get; set; } = new List<WarehouseProductBatchAllocation>();

    [ForeignKey("OrganizationId")]
    [InverseProperty(nameof(Organization.WarehouseProductMovements))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty(nameof(Product.WarehouseProductMovements))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty(nameof(Warehouse.WarehouseProductMovements))]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
