namespace Application.Features.Manual;

public class ModuleSubGroupSelectListDto
{
    public int    Id        { get; set; }
    public string Code      { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string FullName  { get; set; } = null!;
    public List<ModuleSelectListDto> Modules { get; set; } = [];
}

public class ModuleSelectListDto
{
    public int    Id        { get; set; }
    public string Code      { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string FullName  { get; set; } = null!;
}
