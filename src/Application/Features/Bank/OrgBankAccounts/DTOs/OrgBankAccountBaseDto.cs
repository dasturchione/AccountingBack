namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountBaseDto
{
    public int BankId { get; set; }
    public int? BankBranchId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string AccountNumber { get; set; } = null!;
    public short CurrencyId { get; set; }
    public bool IsMain { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateOnly? OpeningBalanceDate { get; set; }
}
