namespace Application.Features.Roles;

public class RoleModuleDto
{
    public int    ModuleId        { get; set; }
    public string ModuleCode      { get; set; } = null!;
    public string ModuleShortName { get; set; } = null!;
    public string ModuleFullName  { get; set; } = null!;
}
