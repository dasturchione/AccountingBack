using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("money_reg_balance")]
[Index("CurrencyId", Name = "idx_money_reg_balance_currency_id")]
[Index("DirectionId", Name = "idx_money_reg_balance_direction_id")]
[Index("DocDate", Name = "idx_money_reg_balance_doc_date")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_money_reg_balance_document")]
[Index("OrganizationId", Name = "idx_money_reg_balance_organization_id")]
[Index("SourceType", "SourceId", Name = "idx_money_reg_balance_source")]
[Index("PostingBatchId", Name = "idx_money_reg_balance_posting_batch_id")]
[Index("ReversalEntryId", Name = "idx_money_reg_balance_reversal_entry_id")]
public partial class MoneyRegisterBalance
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

    [Column("source_type")]
    [StringLength(20)]
    public string SourceType { get; set; } = null!;

    [Column("source_id")]
    public int SourceId { get; set; }

    [Column("direction_id")]
    public short DirectionId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }


    [Column("posting_batch_id")]
    public long? PostingBatchId { get; set; }

    [Column("source_line_id")]
    public long? SourceLineId { get; set; }

    [Column("reversal_entry_id")]
    public long? ReversalEntryId { get; set; }
    [ForeignKey("CurrencyId")]
    [InverseProperty("MoneyRegisterBalances")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("MoneyRegisterBalances")]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey(nameof(DirectionId))]
    [InverseProperty(nameof(MovementDirection.MoneyRegisterBalances))]
    public virtual MovementDirection Direction { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("MoneyRegisterBalances")]
    public virtual Organization Organization { get; set; } = null!;
}
