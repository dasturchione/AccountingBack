namespace Application.Features.Roles;

public class RoleBaseDto
{
    public string     ShortName  { get; set; } = null!;
    public string     FullName   { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool HasGlobalAccess { get; set; }
    public bool IsSystem { get; set; }
    public bool IsOwnerRole { get; set; }
    public int SortOrder { get; set; }
    public List<int> Modules { get; set; } = [];
}
