using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_opening_inventory")]
[Index("CounterpartyId", Name = "ix_inv_opening_inventory_counterparty")]
[Index("DocDate", Name = "ix_inv_opening_inventory_doc_date")]
[Index("StatusId", Name = "ix_inv_opening_inventory_status")]
[Index("WarehouseId", Name = "ix_inv_opening_inventory_warehouse")]
[Index("OrganizationId", "DocNumber", Name = "ux_inv_opening_inventory_org_doc_number", IsUnique = true)]
public partial class InvOpeningInventory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

    [Column("contract_id")]
    public long? ContractId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("total_amount")]
    [Precision(24, 8)]
    public decimal TotalAmount { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey("CancelledByUserId")]
    [InverseProperty("InvOpeningInventoryCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("InvOpeningInventories")]
    public virtual CmnContract? Contract { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("InvOpeningInventories")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<InvOpeningInventoryProduct> InvOpeningInventoryProducts { get; set; } = new List<InvOpeningInventoryProduct>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvOpeningInventories")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("InvOpeningInventoryPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("InvOpeningInventories")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("InvOpeningInventories")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvOpeningInventories")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
