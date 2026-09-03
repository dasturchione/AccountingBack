namespace Application.Features.PaymentAcceptancePoints;

public class PaymentAcceptancePointBaseDto
{
    public short TypeId { get; set; }
    public int? BankAccountId { get; set; }
    public string Name { get; set; } = null!;
    public string? MerchantId { get; set; }
    public string? ExternalId { get; set; }
    public string? SerialNumber { get; set; }
}
