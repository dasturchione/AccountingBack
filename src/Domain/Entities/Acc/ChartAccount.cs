namespace Domain.Entities;

public partial class ChartAccount
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? ParentId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsGroup { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public short? AccountTypeId { get; set; }
    public bool IsQuantity { get; set; }
    public bool IsCurrency { get; set; }
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();
    public virtual ICollection<PostingRuleLine> PostingRuleLineCreditAccounts { get; set; } = new List<PostingRuleLine>();
    public virtual ICollection<PostingRuleLine> PostingRuleLineDebitAccounts { get; set; } = new List<PostingRuleLine>();
    public virtual ICollection<AccountingRegisterEntry> AccRegEntryCreditAccounts { get; set; } = new List<AccountingRegisterEntry>();
    public virtual ICollection<AccountingRegisterEntry> AccRegEntryDebitAccounts { get; set; } = new List<AccountingRegisterEntry>();
    public virtual AccountType? AccountType { get; set; }
    public virtual ICollection<ChartAccount> InverseParent { get; set; } = new List<ChartAccount>();
    public virtual Organization Organization { get; set; } = null!;
    public virtual ChartAccount? Parent { get; set; }
    public virtual State State { get; set; } = null!;
}
