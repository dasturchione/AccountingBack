namespace Domain.Entities;

public partial class RoleModule
{
    public int RoleId { get; set; }

    public int ModuleId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Module Module { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
