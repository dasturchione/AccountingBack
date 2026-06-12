namespace Domain.Entities;

public partial class Language
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string NativeName { get; set; } = null!;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    public virtual State State { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
