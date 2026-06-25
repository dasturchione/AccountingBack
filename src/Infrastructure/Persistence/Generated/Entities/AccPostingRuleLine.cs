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

    [Column("debit_alias")]
    [StringLength(250)]
    public string DebitAlias { get; set; } = null!;

    [Column("credit_alias")]
    [StringLength(250)]
    public string CreditAlias { get; set; } = null!;

    [Column("amount_source")]
    [StringLength(20)]
    public string? AmountSource { get; set; }

    [Column("is_optional")]
    public bool IsOptional { get; set; }

    [ForeignKey("TemplateId")]
    [InverseProperty("AccPostingRuleLines")]
    public virtual AccPostingRule Template { get; set; } = null!;
}
