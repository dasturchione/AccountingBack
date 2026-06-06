namespace Application.Features.Branches;

public class BranchListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? PhoneNumber { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
