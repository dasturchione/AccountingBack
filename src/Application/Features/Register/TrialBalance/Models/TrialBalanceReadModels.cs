namespace Application.Features.TrialBalance;

public class TrialBalanceReadRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
}

public class TrialBalanceReadResult
{
    public List<TrialBalanceReadRow> Rows { get; set; } = [];
}

public class TrialBalanceReadRow
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public string AccountCode { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public short? AccountTypeId { get; set; }
    public decimal OpeningDebitTurnover { get; set; }
    public decimal OpeningCreditTurnover { get; set; }
    public decimal PeriodDebitTurnover { get; set; }
    public decimal PeriodCreditTurnover { get; set; }
}
