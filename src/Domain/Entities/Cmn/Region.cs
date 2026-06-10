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
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();
    public virtual State State { get; set; } = null!;
}
