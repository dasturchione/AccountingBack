namespace Domain.Entities;

public partial class District
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int RegionId { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Region Region { get; set; } = null!;

    public virtual State State { get; set; } = null!;
}
