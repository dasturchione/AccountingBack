using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("marking_transfer")]
[Index("BuyerCounterpartyId", Name = "idx_marking_transfer_buyer_counterparty_id")]
[Index("OrganizationId", Name = "idx_marking_transfer_organization_id")]
[Index("SellerCounterpartyId", Name = "idx_marking_transfer_seller_counterparty_id")]
[Index("OrganizationId", "Status", Name = "idx_marking_transfer_status")]
[Index("TransferDate", Name = "idx_marking_transfer_transfer_date")]
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

    [ForeignKey("BuyerCounterpartyId")]
    [InverseProperty("MarkingTransferBuyerCounterparties")]
    public virtual CounterpartyCard BuyerCounterparty { get; set; } = null!;

    [InverseProperty("MarkingTransfer")]
    public virtual ICollection<MarkingTransferCode> MarkingTransferCodes { get; set; } = new List<MarkingTransferCode>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("MarkingTransfers")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("SellerCounterpartyId")]
    [InverseProperty("MarkingTransferSellerCounterparties")]
    public virtual CounterpartyCard SellerCounterparty { get; set; } = null!;
}
