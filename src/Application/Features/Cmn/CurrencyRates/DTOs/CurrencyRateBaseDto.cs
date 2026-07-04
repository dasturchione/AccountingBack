using SharedKernel.Filters;

namespace Application.Features.Cmn.CurrencyRates;

public class CurrencyRateBaseDto
{
    public short BaseCurrencyId { get; set; }
    public short TargetCurrencyId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public decimal OfficialRate { get; set; }
    public string? RateSource { get; set; }
    public bool IsActive { get; set; } = true;
}
