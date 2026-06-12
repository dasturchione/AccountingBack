namespace Domain.Entities;

public partial class BankAccount
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = null!;
    public short CurrencyId { get; set; }
    public bool IsMain { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Bank Bank { get; set; } = null!;
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();
    public virtual Currency Currency { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}