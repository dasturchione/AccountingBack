namespace Application.Features.ProductGroups;

public class ProductGroupBaseDto
{
    public int? ParentId { get; set; }
    public string Name { get; set; } = null!;
}
