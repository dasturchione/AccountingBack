namespace Domain.Entities;

public partial class ModuleSubGroup
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
}
