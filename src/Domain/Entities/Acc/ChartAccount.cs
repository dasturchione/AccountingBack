using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_chart_account")]
public partial class ChartAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; }

    [Column("number")]
    [StringLength(50)]
    public string Number { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("is_group")]
    public bool IsGroup { get; set; }

    [Column("account_type_id")]
    public short? AccountTypeId { get; set; }

    [Column("is_quantity")]
    public bool IsQuantity { get; set; }

    [Column("is_currency")]
    public bool IsCurrency { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("is_department")]
    public bool IsDepartment { get; set; }

    [Column("is_tax_accounting")]
    public bool IsTaxAccounting { get; set; }

    [Column("is_off_balance")]
    public bool IsOffBalance { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<AccountResolveRule> AccountResolveRules { get; set; } = new List<AccountResolveRule>();

    [InverseProperty("Account")]
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();

    [InverseProperty("CreditAccount")]
    public virtual ICollection<AccountingRegisterEntry> RegisterEntryCreditAccounts { get; set; } = new List<AccountingRegisterEntry>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("ChartAccounts")]
    public virtual Organization Organization { get; set; } = null!;

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

    [ForeignKey("StateId")]
    [InverseProperty("ChartAccounts")]
    public virtual State State { get; set; } = null!;
}
