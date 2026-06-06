namespace Application.Features.Branches;

public class BranchBaseDto
{
    public int OrganizationId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}
