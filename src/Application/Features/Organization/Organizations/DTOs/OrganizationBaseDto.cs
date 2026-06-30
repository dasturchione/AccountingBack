namespace Application.Features.Organizations;

public class OrganizationBaseDto
{
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? Director { get; set; }
    public bool IsParent { get; set; }
    public short? DefaultLanguageId { get; set; }
    public int? TenantId { get; set; }
    public string? SetupStatus { get; set; }
    public DateTime? SetupCompletedAt { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Oked { get; set; }
}
