namespace Domain.Entities;

public partial class PaymentType
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();
}