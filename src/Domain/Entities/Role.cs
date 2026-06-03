namespace Domain.Entities;

public partial class Role
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual State State { get; set; } = null!;

    public virtual ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
