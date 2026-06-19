namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountBaseDto
{
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = null!;
    public short CurrencyId { get; set; }
    public bool IsMain { get; set; }
}
