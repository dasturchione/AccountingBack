using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_warehouse_product_movement")]
[Index("OrganizationId", "MovementDate", Name = "idx_inv_warehouse_product_movement_date")]
[Index("OrganizationId", "DocumentTypeId", "DocumentId", Name = "idx_inv_warehouse_product_movement_document")]
[Index("OrganizationId", "DocumentTypeId", "DocumentId", "DocumentLineId", Name = "idx_inv_warehouse_product_movement_document_line")]
[Index("OrganizationId", "WarehouseId", "ProductId", Name = "idx_inv_warehouse_product_movement_warehouse_product")]
public partial class InvWarehouseProductMovement
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

    [Column("movement_sign")]
    public short MovementSign { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("InvWarehouseProductMovements")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [InverseProperty("ReceiptMovement")]
    public virtual InvWarehouseProductBatch? InvWarehouseProductBatch { get; set; }

    [InverseProperty("IssueMovement")]
    public virtual ICollection<InvWarehouseProductBatchAllocation> InvWarehouseProductBatchAllocations { get; set; } = new List<InvWarehouseProductBatchAllocation>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvWarehouseProductMovements")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvWarehouseProductMovements")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvWarehouseProductMovements")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
