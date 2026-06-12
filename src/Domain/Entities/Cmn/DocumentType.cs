namespace Domain.Entities;

public partial class DocumentType
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<PostingRule> PostingRules { get; set; } = new List<PostingRule>();
    public virtual ICollection<AccountingRegisterEntry> AccountingRegisterEntries { get; set; } = new List<AccountingRegisterEntry>();
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();
    public virtual ICollection<RegisterBalance> RegisterBalances { get; set; } = new List<RegisterBalance>();
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();
    public virtual State State { get; set; } = null!;
}
