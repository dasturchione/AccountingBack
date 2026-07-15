using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_warehouse")]
[Index("BranchId", Name = "idx_inv_warehouse_branch_id")]
[Index("Code", Name = "idx_inv_warehouse_code")]
[Index("IsMain", Name = "idx_inv_warehouse_is_main")]
[Index("OrganizationId", Name = "idx_inv_warehouse_organization_id")]
[Index("ResponsibleUserId", Name = "idx_inv_warehouse_responsible_user_id")]
[Index("StateId", Name = "idx_inv_warehouse_state_id")]
public partial class InvWarehouse
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
    [InverseProperty("InvWarehouses")]
    public virtual OrgBranch? Branch { get; set; }

    [InverseProperty("Warehouse")]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("Warehouse")]
    public virtual ICollection<InvInventoryAdjustmentDoc> InvInventoryAdjustmentDocs { get; set; } = new List<InvInventoryAdjustmentDoc>();

    [InverseProperty("Warehouse")]
    public virtual ICollection<InvInventoryCountDoc> InvInventoryCountDocs { get; set; } = new List<InvInventoryCountDoc>();

    [InverseProperty("Warehouse")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("DestinationWarehouse")]
    public virtual ICollection<InvTransferDoc> InvTransferDocDestinationWarehouses { get; set; } = new List<InvTransferDoc>();

    [InverseProperty("SourceWarehouse")]
    public virtual ICollection<InvTransferDoc> InvTransferDocSourceWarehouses { get; set; } = new List<InvTransferDoc>();

    [InverseProperty("Warehouse")]
    public virtual ICollection<InvWarehouseProductTable> InvWarehouseProductTables { get; set; } = new List<InvWarehouseProductTable>();

    [InverseProperty("Warehouse")]
    public virtual ICollection<InvWarehouseProduct> InvWarehouseProducts { get; set; } = new List<InvWarehouseProduct>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvWarehouses")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Warehouse")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("InvWarehouses")]
    public virtual SysUser? ResponsibleUser { get; set; }

    [InverseProperty("Warehouse")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("InvWarehouses")]
    public virtual CmnState State { get; set; } = null!;
}
