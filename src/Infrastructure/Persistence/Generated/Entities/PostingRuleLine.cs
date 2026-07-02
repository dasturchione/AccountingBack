using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_rule_line")]
public partial class PostingRuleLine
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("template_id")]
    public short TemplateId { get; set; }

    [Column("order_number")]
    public short OrderNumber { get; set; }

    [Column("amount_source")]
    [StringLength(20)]
    public string? AmountSource { get; set; }

    [Column("is_optional")]
    public bool IsOptional { get; set; }

    [Column("debit_alias_id")]
    public short DebitAliasId { get; set; }

    [Column("credit_alias_id")]
    public short CreditAliasId { get; set; }

    [ForeignKey("CreditAliasId")]
    [InverseProperty("PostingRuleLineCreditAliases")]
    public virtual PostingAlias CreditAlias { get; set; } = null!;

    [ForeignKey("DebitAliasId")]
    [InverseProperty("PostingRuleLineDebitAliases")]
    public virtual PostingAlias DebitAlias { get; set; } = null!;

    [ForeignKey("TemplateId")]
    [InverseProperty("PostingRuleLines")]
    public virtual PostingRule Template { get; set; } = null!;
}
