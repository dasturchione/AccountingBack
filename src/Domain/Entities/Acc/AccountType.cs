namespace Domain.Entities;

public partial class AccountType
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<ChartAccount> ChartAccounts { get; set; } = new List<ChartAccount>();
    public virtual State State { get; set; } = null!;
}
