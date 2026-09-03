namespace Application.Features.Manual;

public class ModuleSelectListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
}
