namespace Domain.Entities;

public partial class OperationType
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<PostingRule> PostingRules { get; set; } = new List<PostingRule>();
    public virtual ICollection<AccountingRegisterEntry> AccountingRegisterEntries { get; set; } = new List<AccountingRegisterEntry>();
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();
    public virtual ICollection<InventoryRegisterBalance> InventoryRegisterBalances { get; set; } = new List<InventoryRegisterBalance>();
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();
}
