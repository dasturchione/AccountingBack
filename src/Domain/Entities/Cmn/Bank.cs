namespace Domain.Entities;

public partial class Bank
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Mfo { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();
    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();
}
