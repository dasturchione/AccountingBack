using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_reg_balance")]
[Index("DocDate", Name = "idx_inv_reg_balance_doc_date")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_inv_reg_balance_document")]
[Index("OrganizationId", Name = "idx_inv_reg_balance_organization_id")]
[Index("ProductId", Name = "idx_inv_reg_balance_product_id")]
[Index("WarehouseId", Name = "idx_inv_reg_balance_warehouse_id")]
public partial class InvRegBalance
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Column("quantity")]
    [Precision(18, 3)]
    public decimal Quantity { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("InvRegBalances")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("InvRegBalances")]
    public virtual CmnOperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvRegBalances")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvRegBalances")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvRegBalances")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
