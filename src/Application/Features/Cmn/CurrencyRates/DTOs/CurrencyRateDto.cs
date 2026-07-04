namespace Application.Features.Cmn.CurrencyRates;

public class CurrencyRateDto : CurrencyRateBaseDto
{
    public long Id { get; set; }
    public string BaseCurrencyCode { get; set; } = null!;
    public string BaseCurrencyName { get; set; } = null!;
    public string TargetCurrencyCode { get; set; } = null!;
    public string TargetCurrencyName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
