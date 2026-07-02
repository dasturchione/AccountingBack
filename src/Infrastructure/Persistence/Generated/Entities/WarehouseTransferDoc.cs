using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_transfer_doc")]
[Index("OrganizationId", Name = "idx_inv_transfer_doc_organization_id")]
[Index("DocDate", Name = "idx_inv_transfer_doc_doc_date")]
[Index("SourceWarehouseId", Name = "idx_inv_transfer_doc_source_warehouse_id")]
[Index("DestinationWarehouseId", Name = "idx_inv_transfer_doc_destination_warehouse_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_inv_transfer_doc_doc_number_org", IsUnique = true)]
[Index("StatusId", Name = "idx_inv_transfer_doc_status_id")]
[Index("StateId", Name = "idx_inv_transfer_doc_state_id")]
[Index("PostedByUserId", Name = "idx_inv_transfer_doc_posted_by_user_id")]
[Index("CancelledByUserId", Name = "idx_inv_transfer_doc_cancelled_by_user_id")]
public partial class WarehouseTransferDoc
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

    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("SourceWarehouseId")]
    [InverseProperty("SourceWarehouseTransferDocs")]
    public virtual Warehouse SourceWarehouse { get; set; } = null!;

    [ForeignKey("DestinationWarehouseId")]
    [InverseProperty("DestinationWarehouseTransferDocs")]
    public virtual Warehouse DestinationWarehouse { get; set; } = null!;

    [ForeignKey("StatusId")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("StateId")]
    public virtual State State { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<WarehouseTransferLine> WarehouseTransferLines { get; set; } = new List<WarehouseTransferLine>();
}
