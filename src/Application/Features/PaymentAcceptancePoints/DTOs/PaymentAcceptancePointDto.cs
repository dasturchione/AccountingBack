namespace Application.Features.PaymentAcceptancePoints;

public class PaymentAcceptancePointDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public short TypeId { get; set; }
    public string TypeCode { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public int? BankAccountId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string Name { get; set; } = null!;
    public string? MerchantId { get; set; }
    public string? ExternalId { get; set; }
    public string? SerialNumber { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
