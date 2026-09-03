namespace Application.Features.ProductGroups;

public class ProductGroupBaseDto
{
    public string Code { get; set; } = null!;
    public int? ParentId { get; set; }
    public bool IsAssignable { get; set; } = true;
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
}
