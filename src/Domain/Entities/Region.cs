namespace Domain.Entities;

public partial class Region
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual State State { get; set; } = null!;
}
