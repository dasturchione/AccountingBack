using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_document_type")]
[Index("Code", Name = "idx_cmn_document_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_document_type_state_id")]
public partial class DocumentType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty(nameof(AccountingRegisterEntry.DocumentType))]
    public virtual ICollection<AccountingRegisterEntry> AccountingRegisterEntries { get; set; } = new List<AccountingRegisterEntry>();

    [InverseProperty(nameof(DocumentNumberSequence.DocumentType))]
    public virtual ICollection<DocumentNumberSequence> DocumentNumberSequences { get; set; } = new List<DocumentNumberSequence>();

    [InverseProperty(nameof(WarehouseProductMovement.DocumentType))]
    public virtual ICollection<WarehouseProductMovement> WarehouseProductMovements { get; set; } = new List<WarehouseProductMovement>();

    [InverseProperty(nameof(DocumentTypeTranslation.DocumentType))]
    public virtual ICollection<DocumentTypeTranslation> DocumentTypeTranslations { get; set; } = new List<DocumentTypeTranslation>();

    [InverseProperty(nameof(CounterpartyRegisterBalance.DocumentType))]
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();

    [InverseProperty(nameof(MoneyRegisterBalance.DocumentType))]
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();

    [ForeignKey("StateId")]
    [InverseProperty("DocumentTypes")]
    public virtual State State { get; set; } = null!;
}
