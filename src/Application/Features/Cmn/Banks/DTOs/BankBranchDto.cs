namespace Application.Features.Banks;

public class BankBranchDto
{
    public int Id { get; set; }
    public int BankId { get; set; }
    public string BankCode { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string Mfo { get; set; } = null!;
    public short BranchType { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public DateOnly? OpenedDate { get; set; }
    public DateOnly? SourceUpdatedDate { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? City { get; set; }
    public string? Inn { get; set; }
    public string? Website { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
