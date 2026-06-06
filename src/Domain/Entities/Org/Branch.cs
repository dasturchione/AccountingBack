namespace Domain.Entities;

public partial class Branch
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual Region? Region { get; set; }
    public virtual District? District { get; set; }
    public virtual State State { get; set; } = null!;
}
