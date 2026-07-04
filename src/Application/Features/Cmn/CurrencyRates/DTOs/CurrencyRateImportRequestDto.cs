namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateImportRequestDto
{
    public string? ProviderCode { get; set; }
    public DateTime? Date { get; set; }
}
