namespace Application.Features.Organizations;

public class OrganizationListDto
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RegionId { get; set; }
    public string RegionName { get; set; } = null!;
    public int? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? Director { get; set; }
    public bool IsParent { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public short? DefaultLanguageId { get; set; }
    public string? DefaultLanguageName { get; set; }
    public DateTime CreatedDate { get; set; }
}
