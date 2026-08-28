using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("payment_acceptance_point_operation")]
public sealed class PaymentAcceptancePointOperation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("payment_acceptance_point_id")]
    public int PaymentAcceptancePointId { get; set; }

    [Column("direction_id")]
    public short DirectionId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("exchange_rate")]
    [Precision(18, 6)]
    public decimal ExchangeRate { get; set; }

    [Column("external_transaction_number")]
    [StringLength(150)]
    public string? ExternalTransactionNumber { get; set; }

    [Column("related_document_id")]
    public long? RelatedDocumentId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

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

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.PaymentAcceptancePointOperations))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PaymentAcceptancePointId))]
    [InverseProperty(nameof(Entities.PaymentAcceptancePoint.Operations))]
    public PaymentAcceptancePoint PaymentAcceptancePoint { get; set; } = null!;

    [ForeignKey(nameof(DirectionId))]
    [InverseProperty(nameof(MovementDirection.PaymentAcceptancePointOperations))]
    public MovementDirection Direction { get; set; } = null!;

    [ForeignKey(nameof(CurrencyId))]
    [InverseProperty(nameof(Currency.PaymentAcceptancePointOperations))]
    public Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(StatusId))]
    [InverseProperty(nameof(DocumentStatus.PaymentAcceptancePointOperations))]
    public DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.PaymentAcceptancePointOperations))]
    public State State { get; set; } = null!;

    [ForeignKey(nameof(RelatedDocumentId))]
    [InverseProperty(nameof(DocumentRegistry.PaymentAcceptancePointOperations))]
    public DocumentRegistry? RelatedDocument { get; set; }
}
