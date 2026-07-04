namespace Application.Features.TrialBalance;

public class TrialBalanceFilter
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public bool IncludeZeroBalance { get; set; }
}
