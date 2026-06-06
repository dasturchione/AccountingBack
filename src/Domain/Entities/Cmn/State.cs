namespace Domain.Entities;

public partial class State
{
    public short Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<CounterpartyType> CounterpartyTypes { get; set; } = new List<CounterpartyType>();
    public virtual ICollection<Currency> Currencies { get; set; } = new List<Currency>();
    public virtual ICollection<DocumentStatus> DocumentStatuses { get; set; } = new List<DocumentStatus>();
    public virtual ICollection<District> Districts { get; set; } = new List<District>();
    public virtual ICollection<Language> Languages { get; set; } = new List<Language>();
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    public virtual ICollection<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();
    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();
    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
