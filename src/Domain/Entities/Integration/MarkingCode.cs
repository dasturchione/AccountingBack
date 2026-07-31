using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("marking_code")]
[Index(nameof(OrganizationId), Name = "idx_marking_code_organization_id")]
[Index(nameof(ProductId), Name = "idx_marking_code_product_id")]
[Index(nameof(OrganizationId), nameof(Status), Name = "idx_marking_code_status")]
[Index(nameof(OwnerCounterpartyId), Name = "idx_marking_code_owner_counterparty_id")]
[Index(nameof(WarehouseId), Name = "idx_marking_code_warehouse_id")]
[Index(nameof(ParentMarkingCodeId), Name = "idx_marking_code_parent_marking_code_id")]
[Index(nameof(OrderId), Name = "idx_marking_code_order_id")]
[Index(nameof(UtilizationId), Name = "idx_marking_code_utilization_id")]
[Index(nameof(OrganizationId), nameof(Gtin), nameof(SerialNumber), Name = "ux_marking_code_org_gtin_serial", IsUnique = true)]
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

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey(nameof(OwnerCounterpartyId))]
    public virtual CounterpartyCard? OwnerCounterparty { get; set; }

    [ForeignKey(nameof(WarehouseId))]
    public virtual Warehouse? Warehouse { get; set; }

    [ForeignKey(nameof(ParentMarkingCodeId))]
    public virtual MarkingCode? ParentMarkingCode { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual MarkingOrder? Order { get; set; }

    [ForeignKey(nameof(UtilizationId))]
    public virtual MarkingUtilization? Utilization { get; set; }
}
