using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_transfer_doc")]
[Index("CancelledByUserId", Name = "idx_inv_transfer_doc_cancelled_by_user_id")]
[Index("DestinationWarehouseId", Name = "idx_inv_transfer_doc_destination_warehouse_id")]
[Index("DocDate", Name = "idx_inv_transfer_doc_doc_date")]
[Index("OrganizationId", Name = "idx_inv_transfer_doc_organization_id")]
[Index("PostedByUserId", Name = "idx_inv_transfer_doc_posted_by_user_id")]
[Index("SourceWarehouseId", Name = "idx_inv_transfer_doc_source_warehouse_id")]
[Index("StateId", Name = "idx_inv_transfer_doc_state_id")]
[Index("StatusId", Name = "idx_inv_transfer_doc_status_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_inv_transfer_doc_doc_number_org", IsUnique = true)]
public partial class InvTransferDoc
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

    [Column("source_warehouse_id")]
    public int SourceWarehouseId { get; set; }

    [Column("destination_warehouse_id")]
    public int DestinationWarehouseId { get; set; }

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
    [InverseProperty("InvTransferDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("DestinationWarehouseId")]
    [InverseProperty("InvTransferDocDestinationWarehouses")]
    public virtual InvWarehouse DestinationWarehouse { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<InvTransferLine> InvTransferLines { get; set; } = new List<InvTransferLine>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvTransferDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("InvTransferDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("SourceWarehouseId")]
    [InverseProperty("InvTransferDocSourceWarehouses")]
    public virtual InvWarehouse SourceWarehouse { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("InvTransferDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("InvTransferDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;
}
