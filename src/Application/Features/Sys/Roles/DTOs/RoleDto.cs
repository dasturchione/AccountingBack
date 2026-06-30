namespace Application.Features.Roles;

public class RoleDto
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool HasGlobalAccess { get; set; }
    public bool IsSystem { get; set; }
    public bool IsOwnerRole { get; set; }
    public int SortOrder { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public List<RoleModuleDto> Modules { get; set; } = [];
}
