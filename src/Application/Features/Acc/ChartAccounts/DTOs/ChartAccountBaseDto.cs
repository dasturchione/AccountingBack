namespace Application.Features.ChartAccounts;

public class ChartAccountBaseDto
{
    public int OrganizationId { get; set; }
    public int? ParentId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsGroup { get; set; }
}
