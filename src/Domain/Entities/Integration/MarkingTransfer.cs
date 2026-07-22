using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("marking_transfer")]
[Index(nameof(OrganizationId), Name = "idx_marking_transfer_organization_id")]
[Index(nameof(SellerCounterpartyId), Name = "idx_marking_transfer_seller_counterparty_id")]
[Index(nameof(BuyerCounterpartyId), Name = "idx_marking_transfer_buyer_counterparty_id")]
[Index(nameof(TransferDate), Name = "idx_marking_transfer_transfer_date")]
[Index(nameof(OrganizationId), nameof(Status), Name = "idx_marking_transfer_status")]
[Index(nameof(DocumentId), Name = "idx_marking_transfer_document_id")]
public partial class MarkingTransfer
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("seller_counterparty_id")]
    public int SellerCounterpartyId { get; set; }

    [Column("buyer_counterparty_id")]
    public int BuyerCounterpartyId { get; set; }

    [Column("transfer_date", TypeName = "timestamp without time zone")]
    public DateTime TransferDate { get; set; }

    [Column("document_id")]
    [StringLength(100)]
    public string? DocumentId { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column("external_document_reference")]
    [StringLength(200)]
    public string? ExternalDocumentReference { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(SellerCounterpartyId))]
    public virtual CounterpartyCard SellerCounterparty { get; set; } = null!;

    [ForeignKey(nameof(BuyerCounterpartyId))]
    public virtual CounterpartyCard BuyerCounterparty { get; set; } = null!;

    [InverseProperty(nameof(MarkingTransferCode.MarkingTransfer))]
    public virtual ICollection<MarkingTransferCode> MarkingTransferCodes { get; set; } = new List<MarkingTransferCode>();
}
