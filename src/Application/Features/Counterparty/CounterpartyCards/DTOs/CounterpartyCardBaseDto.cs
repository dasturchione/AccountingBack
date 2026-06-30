namespace Application.Features.CounterpartyCards;

public class CounterpartyCardBaseDto
{
    public short CounterpartyTypeId { get; set; }
    public string? Code { get; set; }
    public bool IsCustomer { get; set; }
    public bool IsSupplier { get; set; }
    public bool IsVatPayer { get; set; }
    public string ShortName { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Inn { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int? RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? Oked { get; set; }
    public string? ExternalId { get; set; }
}
