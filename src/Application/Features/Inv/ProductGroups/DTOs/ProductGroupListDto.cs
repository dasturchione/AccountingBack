namespace Application.Features.ProductGroups;

public class ProductGroupListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string? Code { get; set; }
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
