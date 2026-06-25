namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountCreateResultDto
{
    public int Id { get; set; }
    public string Inn { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
}
