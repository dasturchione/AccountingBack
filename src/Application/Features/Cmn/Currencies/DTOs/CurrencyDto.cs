namespace Application.Features.Cmn.Currencies;

public class CurrencyDto : CurrencyBaseDto
{
    public short Id { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
}
