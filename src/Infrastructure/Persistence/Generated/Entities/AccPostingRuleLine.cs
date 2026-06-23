using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_rule_line")]
[Index("CreditAccountId", Name = "idx_acc_posting_rule_line_credit_account_id")]
[Index("DebitAccountId", Name = "idx_acc_posting_rule_line_debit_account_id")]
[Index("RuleId", Name = "idx_acc_posting_rule_line_rule_id")]
public partial class AccPostingRuleLine
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("rule_id")]
    public int RuleId { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("debit_account_id")]
    public int? DebitAccountId { get; set; }

    [Column("credit_account_id")]
    public int? CreditAccountId { get; set; }

    [Column("amount_source")]
    [StringLength(100)]
    public string AmountSource { get; set; } = null!;

    [Column("quantity_source")]
    [StringLength(100)]
    public string? QuantitySource { get; set; }

    [Column("content_template")]
    [StringLength(1000)]
    public string? ContentTemplate { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("debit_account_source")]
    [StringLength(100)]
    public string? DebitAccountSource { get; set; }

    [Column("credit_account_source")]
    [StringLength(100)]
    public string? CreditAccountSource { get; set; }

    [ForeignKey("CreditAccountId")]
    [InverseProperty("AccPostingRuleLineCreditAccounts")]
    public virtual AccChartAccount? CreditAccount { get; set; }

    [ForeignKey("DebitAccountId")]
    [InverseProperty("AccPostingRuleLineDebitAccounts")]
    public virtual AccChartAccount? DebitAccount { get; set; }

    [ForeignKey("RuleId")]
    [InverseProperty("AccPostingRuleLines")]
    public virtual AccPostingRule Rule { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("AccPostingRuleLines")]
    public virtual CmnState State { get; set; } = null!;
}
