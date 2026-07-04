using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_rule_line")]
public partial class AccPostingRuleLine
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
    [InverseProperty("AccPostingRuleLineCreditAliases")]
    public virtual AccPostingAlias CreditAlias { get; set; } = null!;

    [ForeignKey("DebitAliasId")]
    [InverseProperty("AccPostingRuleLineDebitAliases")]
    public virtual AccPostingAlias DebitAlias { get; set; } = null!;

    [ForeignKey("TemplateId")]
    [InverseProperty("AccPostingRuleLines")]
    public virtual AccPostingRule Template { get; set; } = null!;
}
