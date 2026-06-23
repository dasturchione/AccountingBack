using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_chart_account")]
[Index("AccountTypeId", Name = "idx_acc_chart_account_account_type_id")]
[Index("ParentId", Name = "idx_acc_chart_account_parent_id")]
[Index("StateId", Name = "idx_acc_chart_account_state_id")]
public partial class ChartAccount
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
    public virtual ICollection<AccountResolveRule> AccountResolveRules { get; set; } = new List<AccountResolveRule>();

    [InverseProperty("Account")]
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();

    [InverseProperty("CreditAccount")]
    public virtual ICollection<PostingRuleLine> PostingRuleLineCreditAccounts { get; set; } = new List<PostingRuleLine>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<PostingRuleLine> PostingRuleLineDebitAccounts { get; set; } = new List<PostingRuleLine>();

    [InverseProperty("CreditAccount")]
    public virtual ICollection<AccountingRegisterEntry> RegisterEntryCreditAccounts { get; set; } = new List<AccountingRegisterEntry>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<AccountingRegisterEntry> RegisterEntryDebitAccounts { get; set; } = new List<AccountingRegisterEntry>();

    [ForeignKey("AccountTypeId")]
    [InverseProperty("ChartAccounts")]
    public virtual AccountType? AccountType { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<ChartAccount> InverseParent { get; set; } = new List<ChartAccount>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual ChartAccount? Parent { get; set; }

    [InverseProperty("ExpenseAccount")]
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();

    [ForeignKey("StateId")]
    [InverseProperty("ChartAccounts")]
    public virtual State State { get; set; } = null!;
}
