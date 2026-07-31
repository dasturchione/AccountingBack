using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("marking_code")]
[Index("OrderId", Name = "idx_marking_code_order_id")]
[Index("OrganizationId", Name = "idx_marking_code_organization_id")]
[Index("OwnerCounterpartyId", Name = "idx_marking_code_owner_counterparty_id")]
[Index("ParentMarkingCodeId", Name = "idx_marking_code_parent_marking_code_id")]
[Index("ProductId", Name = "idx_marking_code_product_id")]
[Index("OrganizationId", "Status", Name = "idx_marking_code_status")]
[Index("UtilizationId", Name = "idx_marking_code_utilization_id")]
[Index("WarehouseId", Name = "idx_marking_code_warehouse_id")]
[Index("OrganizationId", "Gtin", "SerialNumber", Name = "ux_marking_code_org_gtin_serial", IsUnique = true)]
public partial class MarkingCode
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("gtin")]
    [StringLength(14)]
    public string Gtin { get; set; } = null!;

    [Column("serial_number")]
    [StringLength(20)]
    public string SerialNumber { get; set; } = null!;

    [Column("check_key")]
    [StringLength(4)]
    public string CheckKey { get; set; } = null!;

    [Column("check_code")]
    [StringLength(44)]
    public string CheckCode { get; set; } = null!;

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("owner_counterparty_id")]
    public int? OwnerCounterpartyId { get; set; }

    [Column("warehouse_id")]
    public int? WarehouseId { get; set; }

    [Column("parent_marking_code_id")]
    public long? ParentMarkingCodeId { get; set; }

    [Column("order_id")]
    public long? OrderId { get; set; }

    [Column("utilization_id")]
    public long? UtilizationId { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("ParentMarkingCode")]
    public virtual ICollection<MarkingCode> InverseParentMarkingCode { get; set; } = new List<MarkingCode>();

    [InverseProperty("ParentMarkingCode")]
    public virtual ICollection<MarkingAggregation> MarkingAggregations { get; set; } = new List<MarkingAggregation>();

    [ForeignKey("OrderId")]
    [InverseProperty("MarkingCodes")]
    public virtual MarkingOrder? Order { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("MarkingCodes")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("OwnerCounterpartyId")]
    [InverseProperty("MarkingCodes")]
    public virtual CounterpartyCard? OwnerCounterparty { get; set; }

    [ForeignKey("ParentMarkingCodeId")]
    [InverseProperty("InverseParentMarkingCode")]
    public virtual MarkingCode? ParentMarkingCode { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("MarkingCodes")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("UtilizationId")]
    [InverseProperty("MarkingCodes")]
    public virtual MarkingUtilization? Utilization { get; set; }

    [ForeignKey("WarehouseId")]
    [InverseProperty("MarkingCodes")]
    public virtual InvWarehouse? Warehouse { get; set; }
}
