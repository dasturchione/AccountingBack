namespace Domain.Entities;

public partial class Bank
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Mfo { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public virtual State State { get; set; } = null!;
}
