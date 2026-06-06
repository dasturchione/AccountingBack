namespace Domain.Entities;

public partial class Organization
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? Director { get; set; }
    public bool IsParent { get; set; }
    public short StateId { get; set; }
    public short? DefaultLanguageId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Region Region { get; set; } = null!;
    public virtual District? District { get; set; }
    public virtual State State { get; set; } = null!;
}
