namespace Application.Features.BankTerminals;

public class BankTerminalBaseDto
{
    public int? BankAccountId { get; set; }
    public string Name { get; set; } = null!;
    public string? MerchantId { get; set; }
    public string? ExternalTerminalId { get; set; }
    public string? SerialNumber { get; set; }
}
