namespace Application.Features.Manual;

public class SelectListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
}

public class ProductSelectListDto : SelectListDto
{
    public string? Mxik { get; set; }
}
