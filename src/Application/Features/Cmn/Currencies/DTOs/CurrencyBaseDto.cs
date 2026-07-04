namespace Application.Features.Cmn.Currencies;

/// <summary>
/// Common currency payload.
/// </summary>
public class CurrencyBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Symbol { get; set; }
}
