using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_document_registry")]
[Index(nameof(DocumentTypeId), nameof(DocumentId), Name = "uq_cmn_document_registry_document", IsUnique = true)]
[Index(nameof(OrganizationId), nameof(DocDate), Name = "ix_cmn_document_registry_organization_date")]
[Index(nameof(OrganizationId), nameof(DocNumber), Name = "ix_cmn_document_registry_doc_number")]
[Index(nameof(StatusId), Name = "ix_cmn_document_registry_status_id")]
public sealed class DocumentRegistry
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("currency_id")]
    public short? CurrencyId { get; set; }

    [Column("status_id")]
    public short? StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.DocumentRegistries))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(DocumentTypeId))]
    [InverseProperty(nameof(DocumentType.DocumentRegistries))]
    public DocumentType DocumentType { get; set; } = null!;

    [ForeignKey(nameof(CurrencyId))]
    [InverseProperty(nameof(Currency.DocumentRegistries))]
    public Currency? Currency { get; set; }

    [ForeignKey(nameof(StatusId))]
    [InverseProperty(nameof(DocumentStatus.DocumentRegistries))]
    public DocumentStatus? Status { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.DocumentRegistries))]
    public State State { get; set; } = null!;

    [InverseProperty(nameof(BankOperation.RelatedDocument))]
    public ICollection<BankOperation> BankOperations { get; set; } = [];

    [InverseProperty(nameof(PaymentAcceptancePointOperation.RelatedDocument))]
    public ICollection<PaymentAcceptancePointOperation> PaymentAcceptancePointOperations { get; set; } = [];

}
