namespace Application.Features.Cmn.CurrencyRevaluations;

public class CurrencyRevaluationBaseDto
{
    public DateTime RevaluationDate { get; set; }
    public DateTime? ProviderRateDate { get; set; }
    public short? TargetCurrencyId { get; set; }
    public bool RevalueAllForeignCurrencies { get; set; } = true;
}
