namespace Application.Features.TrialBalance;

public class TrialBalanceDto
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public bool IncludeZeroBalance { get; set; }
    public decimal OpeningDebitTotal { get; set; }
    public decimal OpeningCreditTotal { get; set; }
    public decimal PeriodDebitTotal { get; set; }
    public decimal PeriodCreditTotal { get; set; }
    public decimal ClosingDebitTotal { get; set; }
    public decimal ClosingCreditTotal { get; set; }
    public List<TrialBalanceItemDto> Items { get; set; } = [];
}

public class TrialBalanceItemDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}
