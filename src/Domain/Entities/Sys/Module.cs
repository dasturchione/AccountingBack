namespace Domain.Entities;

public partial class Module
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int SubGroupId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ModuleSubGroup SubGroup { get; set; } = null!;
    public virtual ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();
}
