using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sale_shipment_doc")]
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
    [InverseProperty(nameof(User.SaleShipmentDocAcceptedUsers))]
    public virtual User? AcceptedUser { get; set; }

    [ForeignKey("CancelledUserId")]
    [InverseProperty(nameof(User.SaleShipmentDocCancelledUsers))]
    public virtual User? CancelledUser { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty(nameof(CounterpartyCard.SaleShipmentDocs))]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CreatedUserId")]
    [InverseProperty(nameof(User.SaleShipmentDocCreatedUsers))]
    public virtual User CreatedUser { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty(nameof(Organization.SaleShipmentDocs))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("SaleDocId")]
    [InverseProperty(nameof(SaleDoc.SaleShipmentDocs))]
    public virtual SaleDoc? SaleDoc { get; set; }

    [InverseProperty(nameof(SaleShipmentProduct.Owner))]
    public virtual ICollection<SaleShipmentProduct> SaleShipmentProducts { get; set; } = new List<SaleShipmentProduct>();

    [ForeignKey("StatusId")]
    [InverseProperty(nameof(DocumentStatus.SaleShipmentDocs))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("SubmittedUserId")]
    [InverseProperty(nameof(User.SaleShipmentDocSubmittedUsers))]
    public virtual User? SubmittedUser { get; set; }

    [ForeignKey("WarehouseId")]
    [InverseProperty(nameof(Warehouse.SaleShipmentDocs))]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
