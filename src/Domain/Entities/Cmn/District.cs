namespace Domain.Entities;

public partial class District
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int RegionId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();
    public virtual Region Region { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}
