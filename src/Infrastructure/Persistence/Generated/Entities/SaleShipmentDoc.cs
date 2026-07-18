using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sale_shipment_doc")]
[Index("CreatedUserId", Name = "idx_sale_shipment_doc_created_user_id")]
[Index("OrganizationId", "StatusId", "DocDate", Name = "idx_sale_shipment_doc_organization_status_date", IsDescending = new[] { false, false, true })]
[Index("OrganizationId", "WarehouseId", "DocDate", Name = "idx_sale_shipment_doc_organization_warehouse_date", IsDescending = new[] { false, false, true })]
public partial class SaleShipmentDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("sale_doc_id")]
    public long? SaleDocId { get; set; }

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("doc_number")]
    [StringLength(50)]
    public string? DocNumber { get; set; }

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("submitted_at", TypeName = "timestamp without time zone")]
    public DateTime? SubmittedAt { get; set; }

    [Column("submitted_user_id")]
    public int? SubmittedUserId { get; set; }

    [Column("accepted_at", TypeName = "timestamp without time zone")]
    public DateTime? AcceptedAt { get; set; }

    [Column("accepted_user_id")]
    public int? AcceptedUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_user_id")]
    public int? CancelledUserId { get; set; }

    [Column("created_user_id")]
    public int CreatedUserId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("AcceptedUserId")]
    [InverseProperty("SaleShipmentDocAcceptedUsers")]
    public virtual SysUser? AcceptedUser { get; set; }

    [ForeignKey("CancelledUserId")]
    [InverseProperty("SaleShipmentDocCancelledUsers")]
    public virtual SysUser? CancelledUser { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("SaleShipmentDocs")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CreatedUserId")]
    [InverseProperty("SaleShipmentDocCreatedUsers")]
    public virtual SysUser CreatedUser { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("SaleShipmentDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("SaleDocId")]
    [InverseProperty("SaleShipmentDocs")]
    public virtual SaleDoc? SaleDoc { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<SaleShipmentProduct> SaleShipmentProducts { get; set; } = new List<SaleShipmentProduct>();

    [ForeignKey("StatusId")]
    [InverseProperty("SaleShipmentDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("SubmittedUserId")]
    [InverseProperty("SaleShipmentDocSubmittedUsers")]
    public virtual SysUser? SubmittedUser { get; set; }

    [ForeignKey("WarehouseId")]
    [InverseProperty("SaleShipmentDocs")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
