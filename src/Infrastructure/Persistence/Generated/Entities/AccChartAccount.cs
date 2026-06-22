using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_chart_account")]
[Index("AccountTypeId", Name = "idx_acc_chart_account_account_type_id")]
[Index("ParentId", Name = "idx_acc_chart_account_parent_id")]
[Index("StateId", Name = "idx_acc_chart_account_state_id")]
public partial class AccChartAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("is_group")]
    public bool IsGroup { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("account_type_id")]
    public short? AccountTypeId { get; set; }

    [Column("is_quantity")]
    public bool IsQuantity { get; set; }

    [Column("is_currency")]
    public bool IsCurrency { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<AccAccountResolveRule> AccAccountResolveRules { get; set; } = new List<AccAccountResolveRule>();

    [InverseProperty("Account")]
    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    [InverseProperty("CreditAccount")]
    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLineCreditAccounts { get; set; } = new List<AccPostingRuleLine>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLineDebitAccounts { get; set; } = new List<AccPostingRuleLine>();

    [InverseProperty("CreditAccount")]
    public virtual ICollection<AccRegEntry> AccRegEntryCreditAccounts { get; set; } = new List<AccRegEntry>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<AccRegEntry> AccRegEntryDebitAccounts { get; set; } = new List<AccRegEntry>();

    [ForeignKey("AccountTypeId")]
    [InverseProperty("AccChartAccounts")]
    public virtual AccAccountType? AccountType { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<AccChartAccount> InverseParent { get; set; } = new List<AccChartAccount>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual AccChartAccount? Parent { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("AccChartAccounts")]
    public virtual CmnState State { get; set; } = null!;
}
