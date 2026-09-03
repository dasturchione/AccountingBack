namespace Application.Features.Cmn.CurrencyRevaluations;

public class CurrencyRevaluationLineDto
{
    public short BaseCurrencyId { get; set; }
    public short TargetCurrencyId { get; set; }
    public string TargetCurrencyCode { get; set; } = null!;
    public decimal BalanceAmount { get; set; }
    public decimal OpeningRate { get; set; }
    public decimal CurrentRate { get; set; }
    public decimal DifferenceAmount { get; set; }
}
