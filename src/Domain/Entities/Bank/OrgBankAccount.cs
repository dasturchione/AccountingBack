namespace Domain.Entities;

public partial class OrgBankAccount
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = null!;
    public short CurrencyId { get; set; }
    public bool IsMain { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
}
