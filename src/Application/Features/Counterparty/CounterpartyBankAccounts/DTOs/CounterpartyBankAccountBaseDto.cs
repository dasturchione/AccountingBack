namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountBaseDto
{
    public int CounterpartyId { get; set; }
    public int BankId { get; set; }
    public int? BankBranchId { get; set; }
    public string AccountNumber { get; set; } = null!;
    public short CurrencyId { get; set; }
    public bool IsMain { get; set; }
}
