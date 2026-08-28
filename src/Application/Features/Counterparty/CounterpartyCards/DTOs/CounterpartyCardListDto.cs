namespace Application.Features.CounterpartyCards;

public class CounterpartyCardListDto
{
    public int Id { get; set; } 
    public int OrganizationId { get; set; } 
    public string OrganizationName { get; set; } = null!;
    public string? Code { get; set; }
    public bool IsVatPayer { get; set; }
    public string ShortName { get; set; } = null!; 
    public string? FullName { get; set; } 
    public string? Inn { get; set; } 
    public string? Email { get; set; } 
    public string? PhoneNumber { get; set; } 
    public int? RegionId { get; set; } 
    public string? RegionName { get; set; } 
    public string? Address { get; set; } 
    public int? DistrictId { get; set; } 
    public string? DistrictName { get; set; } 
    public short StateId { get; set; } 
    public string StateName { get; set; } = null!; 
    public string? Oked { get; set; }
    public string? ExternalId { get; set; }
    public DateTime CreatedDate { get; set; } 
}
