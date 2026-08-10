namespace Application.Features.BankTerminals;

public class BankTerminalListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? BankAccountId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string Name { get; set; } = null!;
    public string? MerchantId { get; set; }
    public string? ExternalTerminalId { get; set; }
    public string? SerialNumber { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
